param(
    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [string]$MarkdownOut,

    [switch]$IncludeExe,

    [switch]$Recurse
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedOutputDirectory = (Resolve-Path $OutputDirectory).Path

if ($Recurse.IsPresent) {
    $files = @(Get-ChildItem -Path $resolvedOutputDirectory -Recurse -File)
    $scanMode = 'Recursive'
}
else {
    $files = @(Get-ChildItem -Path $resolvedOutputDirectory -File)
    $scanMode = 'TopDirectoryOnly'
}

$matches = @(
    $files | Where-Object {
        $_.Name -like 'dk.gi*.dll' -or ($IncludeExe.IsPresent -and $_.Name -like 'dk.gi*.exe')
    }
)

$status = if (@($matches).Count -eq 0) { 'PASS' } else { 'FAIL' }

Write-Host "GI runtime verification: $status"
Write-Host "Output directory: $resolvedOutputDirectory"
Write-Host "Scan mode: $scanMode"
Write-Host "Include exe artifacts: $($IncludeExe.IsPresent)"
Write-Host "GI assemblies found: $(@($matches).Count)"

if (@($matches).Count -gt 0) {
    Write-Host ""
    Write-Host "Found GI runtime artifacts:"
    foreach ($m in $matches) {
        Write-Host " - $($m.FullName)"
    }
}

if ($MarkdownOut) {
    $lines = @()
    $lines += "# GI runtime verification"
    $lines += ""
    $lines += "- Status: $status"
    $lines += "- Output directory: $resolvedOutputDirectory"
    $lines += "- Scan mode: $scanMode"
    $lines += "- Include exe artifacts: $($IncludeExe.IsPresent)"
    $lines += "- GI assemblies found: $(@($matches).Count)"
    $lines += ""

    if (@($matches).Count -gt 0) {
        $lines += "## Found GI runtime artifacts"
        foreach ($m in $matches) {
            $lines += "- $($m.FullName)"
        }
    }
    else {
        $lines += "No dk.gi*.dll files found in the selected output directory."
    }

    $reportDir = Split-Path $MarkdownOut -Parent
    if ($reportDir -and -not (Test-Path $reportDir)) {
        New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    }

    Set-Content -Path $MarkdownOut -Value $lines -Encoding UTF8
    Write-Host ""
    Write-Host "Markdown report written to: $((Resolve-Path $MarkdownOut).Path)"
}

if ($status -eq 'FAIL') {
    exit 1
}
exit 0
