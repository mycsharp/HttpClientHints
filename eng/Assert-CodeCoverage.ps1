# Fails when the line or branch coverage of a package is below the threshold, or a package is missing.
# Usage: ./eng/Assert-CodeCoverage.ps1 -ResultsDirectory ./TestResults -Threshold 100
#
# Every test project and target framework writes its own Cobertura report. The report that dotnet test merges from
# them does not combine the hits of a package that several test projects cover: it keeps the hits of one project only.
# This script therefore reads all reports and takes, for every line, the highest number of covered branches of any
# report. Cobertura does not record which branches are covered, so the branch coverage is a lower bound.

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory,

    [ValidateRange(0, 100)]
    [double] $Threshold = 100,

    [ValidateNotNullOrEmpty()]
    [string[]] $RequiredPackages = @('MyCSharp.HttpClientHints', 'MyCSharp.HttpClientHints.AspNetCore')
)

$ErrorActionPreference = 'Stop'

$reports = @(Get-ChildItem -Path $ResultsDirectory -Recurse -Filter '*.cobertura.xml')
if ($reports.Count -eq 0) {
    throw "Expected Cobertura reports in '$ResultsDirectory', found none."
}

# package -> "file:line" -> @{ Hit; Covered; Total }
$packages = @{}
foreach ($name in $RequiredPackages) {
    $packages[$name] = @{}
}

foreach ($report in $reports) {
    [xml] $coverage = Get-Content -LiteralPath $report.FullName -Raw
    foreach ($package in @($coverage.coverage.packages.package)) {
        if (-not $packages.ContainsKey($package.name)) {
            continue
        }

        $lines = $packages[$package.name]
        foreach ($class in @($package.classes.class)) {
            foreach ($line in @($class.lines.line)) {
                if ($null -eq $line) {
                    continue
                }

                $key = "$($class.filename):$($line.number)"
                $covered = 0
                $total = 0
                if ($line.'condition-coverage' -match '\((\d+)/(\d+)\)') {
                    $covered = [int] $Matches[1]
                    $total = [int] $Matches[2]
                }

                $entry = $lines[$key]
                if ($null -eq $entry) {
                    $lines[$key] = @{ Hit = [int] $line.hits -gt 0; Covered = $covered; Total = $total }
                    continue
                }

                $entry.Hit = $entry.Hit -or ([int] $line.hits -gt 0)
                $entry.Covered = [Math]::Max($entry.Covered, $covered)
                $entry.Total = [Math]::Max($entry.Total, $total)
            }
        }
    }
}

$invariant = [cultureinfo]::InvariantCulture
Write-Host ([string]::Format($invariant, 'Coverage of {0} reports in {1}', $reports.Count, $ResultsDirectory))

function Get-Rate([double] $covered, [double] $total) {
    if ($total -eq 0) { return 100.0 }
    return $covered * 100 / $total
}

$failed = $false
$allLines = 0; $allCoveredLines = 0; $allBranches = 0; $allCoveredBranches = 0
foreach ($name in $RequiredPackages) {
    $entries = @($packages[$name].Values)
    if ($entries.Count -eq 0) {
        Write-Host "Package $name`: missing from the coverage reports"
        $failed = $true
        continue
    }

    $coveredLines = @($entries | Where-Object Hit).Count
    $branches = ($entries | Measure-Object -Property Total -Sum).Sum
    $coveredBranches = ($entries | Measure-Object -Property Covered -Sum).Sum
    $allLines += $entries.Count; $allCoveredLines += $coveredLines
    $allBranches += $branches; $allCoveredBranches += $coveredBranches

    $lineRate = Get-Rate $coveredLines $entries.Count
    $branchRate = Get-Rate $coveredBranches $branches
    Write-Host ([string]::Format($invariant, 'Package {0}: line {1:F2} % ({2}/{3}), branch {4:F2} % ({5}/{6})',
        $name, $lineRate, $coveredLines, $entries.Count, $branchRate, $coveredBranches, $branches))

    if ($lineRate -lt $Threshold -or $branchRate -lt $Threshold) {
        $failed = $true
        foreach ($key in ($packages[$name].Keys | Sort-Object)) {
            $entry = $packages[$name][$key]
            if (-not $entry.Hit -or $entry.Covered -lt $entry.Total) {
                Write-Host ([string]::Format($invariant, '  not covered: {0} (branches {1}/{2})', $key, $entry.Covered, $entry.Total))
            }
        }
    }
}

Write-Host ([string]::Format($invariant, 'Line coverage:   {0:F2} % (minimum {1} %)', (Get-Rate $allCoveredLines $allLines), $Threshold))
Write-Host ([string]::Format($invariant, 'Branch coverage: {0:F2} % (minimum {1} %)', (Get-Rate $allCoveredBranches $allBranches), $Threshold))

if ($failed) {
    Write-Error ([string]::Format($invariant, 'Code coverage is below the minimum of {0} % or a required package is missing.', $Threshold))
    exit 1
}
