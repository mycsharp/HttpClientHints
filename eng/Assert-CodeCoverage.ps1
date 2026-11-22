# Fails when the merged Cobertura report in the results directory is below the coverage threshold
# or does not contain every required package.
# Usage: ./eng/Assert-CodeCoverage.ps1 -ResultsDirectory ./TestResults -Threshold 100

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
$report = $reports | Where-Object Name -like 'merged-*' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $report -and $reports.Count -eq 1) {
    $report = $reports[0]
}
if (-not $report) {
    throw "Expected one merged Cobertura report in '$ResultsDirectory', found $($reports.Count) reports."
}

[xml] $coverage = Get-Content -Path $report.FullName -Raw
$invariant = [cultureinfo]::InvariantCulture
$lineRate = [double]::Parse($coverage.coverage.'line-rate', $invariant) * 100
$branchRate = [double]::Parse($coverage.coverage.'branch-rate', $invariant) * 100

Write-Host ([string]::Format($invariant, 'Coverage report: {0}', $report.FullName))
Write-Host ([string]::Format($invariant, 'Line coverage:   {0:F2} % (minimum {1} %)', $lineRate, $Threshold))
Write-Host ([string]::Format($invariant, 'Branch coverage: {0:F2} % (minimum {1} %)', $branchRate, $Threshold))

$failed = $lineRate -lt $Threshold -or $branchRate -lt $Threshold

# A report without one of the packages would otherwise pass with the coverage of the remaining packages.
$packages = @($coverage.coverage.packages.package)
foreach ($name in $RequiredPackages) {
    $found = @($packages | Where-Object { $_.name -eq $name })
    if ($found.Count -eq 0) {
        Write-Host "Package $name`: missing from the coverage report"
        $failed = $true
        continue
    }

    foreach ($package in $found) {
        $packageLineRate = [double]::Parse($package.'line-rate', $invariant) * 100
        $packageBranchRate = [double]::Parse($package.'branch-rate', $invariant) * 100
        Write-Host ([string]::Format($invariant, 'Package {0}: line {1:F2} %, branch {2:F2} %', $name, $packageLineRate, $packageBranchRate))

        if ($packageLineRate -lt $Threshold -or $packageBranchRate -lt $Threshold) {
            $failed = $true
        }
    }
}

if ($failed) {
    Write-Error ([string]::Format($invariant, 'Code coverage is below the minimum of {0} % or a required package is missing.', $Threshold))
    exit 1
}
