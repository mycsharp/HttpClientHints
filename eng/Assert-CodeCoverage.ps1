# Fails when the line or branch coverage of a package is below the threshold, or a package is missing.
# Usage: ./eng/Assert-CodeCoverage.ps1 -ResultsDirectory ./TestResults -Threshold 100
#
# Every test project and target framework writes its own Cobertura report. The report that dotnet test merges from
# them does not combine the hits of a package that several test projects cover: it keeps the hits of one project only.
# This script therefore reads all reports in the results directory, so run the tests into an empty directory.
#
# A line counts as covered if any report hits it. Lines are identified per class, because one source line can contain
# code of several classes, for example a method and a lambda. For the branches of a line, the highest number of covered
# branches of any report counts; lines whose number of branches differs between target frameworks are counted for
# each number. Cobertura does not record which branches are covered, so the branch coverage is a lower bound.

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

# package -> "file|class|line" -> hit, and package -> "file|class|line|branches" -> highest number of covered branches
$lines = @{}
$branches = @{}
foreach ($name in $RequiredPackages) {
    $lines[$name] = @{}
    $branches[$name] = @{}
}

foreach ($report in $reports) {
    [xml] $coverage = Get-Content -LiteralPath $report.FullName -Raw
    foreach ($package in @($coverage.coverage.packages.package)) {
        if ($null -eq $package -or -not $lines.ContainsKey($package.name)) {
            continue
        }

        foreach ($class in @($package.classes.class)) {
            if ($null -eq $class) {
                continue
            }

            foreach ($line in @($class.lines.line)) {
                if ($null -eq $line) {
                    continue
                }

                $lineKey = "$($class.filename)|$($class.name)|$($line.number)"
                $lines[$package.name][$lineKey] = [bool] $lines[$package.name][$lineKey] -or ([int] $line.hits -gt 0)

                if ($line.'condition-coverage' -match '\((\d+)/(\d+)\)') {
                    $branchKey = "$lineKey|$($Matches[2])"
                    $covered = [int] $Matches[1]
                    $previous = $branches[$package.name][$branchKey]
                    if ($null -eq $previous -or $covered -gt $previous) {
                        $branches[$package.name][$branchKey] = $covered
                    }
                }
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
    if ($lines[$name].Count -eq 0) {
        Write-Host "Package $name`: missing from the coverage reports"
        $failed = $true
        continue
    }

    $lineCount = $lines[$name].Count
    $coveredLines = @($lines[$name].Values | Where-Object { $_ }).Count
    $branchCount = 0
    $coveredBranches = 0
    foreach ($key in $branches[$name].Keys) {
        $branchCount += [int] $key.Substring($key.LastIndexOf('|') + 1)
        $coveredBranches += $branches[$name][$key]
    }

    $allLines += $lineCount; $allCoveredLines += $coveredLines
    $allBranches += $branchCount; $allCoveredBranches += $coveredBranches

    $lineRate = Get-Rate $coveredLines $lineCount
    $branchRate = Get-Rate $coveredBranches $branchCount
    Write-Host ([string]::Format($invariant, 'Package {0}: line {1:F2} % ({2}/{3}), branch {4:F2} % ({5}/{6})',
        $name, $lineRate, $coveredLines, $lineCount, $branchRate, $coveredBranches, $branchCount))

    if ($lineRate -lt $Threshold -or $branchRate -lt $Threshold) {
        $failed = $true
        foreach ($key in ($lines[$name].Keys | Sort-Object)) {
            if (-not $lines[$name][$key]) {
                Write-Host "  line not covered: $key"
            }
        }

        foreach ($key in ($branches[$name].Keys | Sort-Object)) {
            $total = [int] $key.Substring($key.LastIndexOf('|') + 1)
            if ($branches[$name][$key] -lt $total) {
                Write-Host ([string]::Format($invariant, '  branches not covered: {0} ({1}/{2})',
                    $key.Substring(0, $key.LastIndexOf('|')), $branches[$name][$key], $total))
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
