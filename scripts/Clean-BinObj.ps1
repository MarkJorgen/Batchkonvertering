param(
    [string]$SolutionRoot = ".",
    [switch]$WhatIf
)

$solutionPath = Resolve-Path $SolutionRoot
Write-Host "Scanner under: $solutionPath"

$folders = Get-ChildItem -Path $solutionPath -Directory -Recurse -Force |
    Where-Object { $_.Name -in @("bin", "obj") }

if (-not $folders) {
    Write-Host "Ingen bin/obj-mapper fundet."
    exit 0
}

foreach ($folder in $folders) {
    if ($WhatIf) {
        Write-Host "[WHATIF] Ville slette: $($folder.FullName)"
    }
    else {
        Write-Host "Sletter: $($folder.FullName)"
        Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
    }
}

Write-Host "Færdig."