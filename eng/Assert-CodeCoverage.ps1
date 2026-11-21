# Fails when the merged Cobertura report in the results directory is below the coverage threshold.
# Usage: ./eng/Assert-CodeCoverage.ps1 -ResultsDirectory ./TestResults -Threshold 90

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory,

    [ValidateRange(0, 100)]
    [double] $Threshold = 90
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

if ($lineRate -lt $Threshold -or $branchRate -lt $Threshold) {
    Write-Error ([string]::Format($invariant, 'Code coverage is below the minimum of {0} %.', $Threshold))
    exit 1
}
