[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $JobName,

    [string] $Configuration = "Debug",

    [string] $TargetFramework = "net48",

    [string] $RepoRoot = (Get-Location).Path,

    [string] $RuntimeOutputPath,

    [switch] $FailOnGiArtifacts
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Info {
    param([string] $Message)
    Write-Host "[INFO] $Message"
}

function Write-WarnLine {
    param([string] $Message)
    Write-Warning $Message
}

function Get-RelativePathSafe {
    param(
        [Parameter(Mandatory = $true)][string] $BasePath,
        [Parameter(Mandatory = $true)][string] $TargetPath
    )

    $base = [System.IO.Path]::GetFullPath($BasePath)
    if (-not $base.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $base += [System.IO.Path]::DirectorySeparatorChar
    }

    $target = [System.IO.Path]::GetFullPath($TargetPath)

    $baseUri = New-Object System.Uri($base)
    $targetUri = New-Object System.Uri($target)
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)

    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', '\')
}

function Get-FileSha256 {
    param([string] $Path)

    try {
        return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
    }
    catch {
        return ""
    }
}

function Resolve-DefaultRuntimeOutputPath {
    param(
        [string] $RepoRoot,
        [string] $JobName,
        [string] $Configuration,
        [string] $TargetFramework
    )

    $candidates = @(
        (Join-Path $RepoRoot "jobs\$JobName\$JobName\bin\$Configuration\$TargetFramework"),
        (Join-Path $RepoRoot "artifacts\bin\$JobName\$Configuration\$TargetFramework"),
        (Join-Path $RepoRoot "artifacts\bin\$JobName")
    ) | Select-Object -Unique

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    $jobsRoot = Join-Path $RepoRoot "jobs"
    if (Test-Path -LiteralPath $jobsRoot) {
        $existingBins = @(
            Get-ChildItem -Path $jobsRoot -Directory -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -like "*\$JobName\bin\$Configuration\*" -and
                $_.FullName -like "*\$TargetFramework"
            }
        )

        if ($existingBins.Count -eq 1) {
            return $existingBins[0].FullName
        }
    }

    throw "Kunne ikke finde runtime output mappe for job '$JobName'. Angiv -RuntimeOutputPath eksplicit."
}

$resolvedRepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path

if ([string]::IsNullOrWhiteSpace($RuntimeOutputPath)) {
    $resolvedRuntimeOutputPath = Resolve-DefaultRuntimeOutputPath `
        -RepoRoot $resolvedRepoRoot `
        -JobName $JobName `
        -Configuration $Configuration `
        -TargetFramework $TargetFramework
}
else {
    if (-not (Test-Path -LiteralPath $RuntimeOutputPath)) {
        throw "Angivet RuntimeOutputPath findes ikke: $RuntimeOutputPath"
    }

    $resolvedRuntimeOutputPath = (Resolve-Path -LiteralPath $RuntimeOutputPath).Path
}

if (-not (Test-Path -LiteralPath $resolvedRuntimeOutputPath)) {
    throw "Runtime output mappe findes ikke: $resolvedRuntimeOutputPath"
}

$evidenceDir = Join-Path $resolvedRepoRoot "docs\jobs\evidence\$JobName"
New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null

$timestamp = Get-Date
$stamp = $timestamp.ToString("yyyyMMdd-HHmmss")

$jsonReportPath = Join-Path $evidenceDir "gi-runtime-artifact-verification-$stamp.json"
$mdReportPath   = Join-Path $evidenceDir "gi-runtime-artifact-verification-$stamp.md"

Write-Info "Repo root: $resolvedRepoRoot"
Write-Info "Job: $JobName"
Write-Info "Runtime output: $resolvedRuntimeOutputPath"

$allFiles = @(
    Get-ChildItem -LiteralPath $resolvedRuntimeOutputPath -File -Recurse |
    Sort-Object FullName
)

$allAssemblies = @(
    $allFiles | Where-Object {
        $_.Extension -in @(".dll", ".exe")
    }
)

$giAssemblies = @(
    $allAssemblies | Where-Object {
        $_.Name -match '^(dk\.gi.*)\.dll$'
    }
)

$interestingAssemblies = @(
    $allAssemblies | Where-Object {
        $_.Name -match '^(dk\.gi.*|Microsoft\.Xrm.*|Microsoft\.PowerPlatform.*|Microsoft\.CrmSdk.*|Microsoft\.IdentityModel.*)\.(dll|exe)$'
    }
)

$topLevelFiles = @(
    Get-ChildItem -LiteralPath $resolvedRuntimeOutputPath -File | Sort-Object Name
)

$topLevelAssemblies = @(
    $topLevelFiles | Where-Object { $_.Extension -in @(".dll", ".exe") }
)

$topLevelGiAssemblies = @(
    $topLevelAssemblies | Where-Object {
        $_.Name -match '^(dk\.gi.*)\.dll$'
    }
)

$report = [ordered]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    repoRoot = $resolvedRepoRoot
    jobName = $JobName
    configuration = $Configuration
    targetFramework = $TargetFramework
    runtimeOutputPath = $resolvedRuntimeOutputPath
    totalFiles = $allFiles.Count
    totalAssemblies = $allAssemblies.Count
    topLevelAssemblyCount = $topLevelAssemblies.Count
    giAssemblyCount = $giAssemblies.Count
    topLevelGiAssemblyCount = $topLevelGiAssemblies.Count
    giAssembliesPresent = ($giAssemblies.Count -gt 0)
    topLevelGiAssembliesPresent = ($topLevelGiAssemblies.Count -gt 0)
    giAssemblies = @(
        foreach ($item in $giAssemblies) {
            [ordered]@{
                name = $item.Name
                relativePath = Get-RelativePathSafe -BasePath $resolvedRuntimeOutputPath -TargetPath $item.FullName
                length = $item.Length
                lastWriteTimeUtc = $item.LastWriteTimeUtc.ToString("o")
                sha256 = Get-FileSha256 -Path $item.FullName
            }
        }
    )
    topLevelAssemblies = @(
        foreach ($item in $topLevelAssemblies) {
            [ordered]@{
                name = $item.Name
                length = $item.Length
                lastWriteTimeUtc = $item.LastWriteTimeUtc.ToString("o")
                sha256 = Get-FileSha256 -Path $item.FullName
            }
        }
    )
    interestingAssemblies = @(
        foreach ($item in $interestingAssemblies) {
            [ordered]@{
                name = $item.Name
                relativePath = Get-RelativePathSafe -BasePath $resolvedRuntimeOutputPath -TargetPath $item.FullName
            }
        }
    )
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonReportPath -Encoding UTF8

if ($giAssemblies.Count -eq 0) {
    $giSummary = "Ingen dk.gi assemblies fundet i runtime output."
}
else {
    $giSummary = "Der blev fundet $($giAssemblies.Count) dk.gi assembly/artifact(er) i runtime output."
}

if ($topLevelGiAssemblies.Count -eq 0) {
    $topLevelGiSummary = "Ingen dk.gi assemblies fundet i top-level runtime output."
}
else {
    $topLevelGiSummary = "Der blev fundet $($topLevelGiAssemblies.Count) dk.gi assembly/artifact(er) i top-level runtime output."
}

$md = New-Object System.Text.StringBuilder
[void]$md.AppendLine("# GI runtime artifact verification")
[void]$md.AppendLine()
[void]$md.AppendLine("- Generated: " + $timestamp.ToString("yyyy-MM-dd HH:mm:ss K"))
[void]$md.AppendLine("- Job: " + $JobName)
[void]$md.AppendLine("- Configuration: " + $Configuration)
[void]$md.AppendLine("- TargetFramework: " + $TargetFramework)
[void]$md.AppendLine("- Runtime output: " + $resolvedRuntimeOutputPath)
[void]$md.AppendLine("- Total files: " + $allFiles.Count)
[void]$md.AppendLine("- Total assemblies (.dll/.exe): " + $allAssemblies.Count)
[void]$md.AppendLine("- Top-level assemblies: " + $topLevelAssemblies.Count)
[void]$md.AppendLine("- GI assemblies found: " + $giAssemblies.Count)
[void]$md.AppendLine("- Top-level GI assemblies found: " + $topLevelGiAssemblies.Count)
[void]$md.AppendLine()
[void]$md.AppendLine("## Konklusion")
[void]$md.AppendLine()
[void]$md.AppendLine("- " + $giSummary)
[void]$md.AppendLine("- " + $topLevelGiSummary)
[void]$md.AppendLine()
[void]$md.AppendLine("## Top-level assemblies")
[void]$md.AppendLine()

if ($topLevelAssemblies.Count -eq 0) {
    [void]$md.AppendLine("_Ingen top-level assemblies fundet._")
}
else {
    foreach ($item in $topLevelAssemblies) {
        [void]$md.AppendLine("- " + $item.Name)
    }
}

[void]$md.AppendLine()
[void]$md.AppendLine("## GI assemblies")
[void]$md.AppendLine()

if ($giAssemblies.Count -eq 0) {
    [void]$md.AppendLine("_Ingen dk.gi assemblies fundet._")
}
else {
    foreach ($item in $giAssemblies) {
        $relative = Get-RelativePathSafe -BasePath $resolvedRuntimeOutputPath -TargetPath $item.FullName
        [void]$md.AppendLine("- " + $relative)
    }
}

[void]$md.AppendLine()
[void]$md.AppendLine("## JSON evidence")
[void]$md.AppendLine()
[void]$md.AppendLine("- " + (Split-Path -Leaf $jsonReportPath))

$md.ToString() | Set-Content -LiteralPath $mdReportPath -Encoding UTF8

Write-Info "Markdown report skrevet: $mdReportPath"
Write-Info "JSON report skrevet: $jsonReportPath"

if ($giAssemblies.Count -gt 0) {
    Write-WarnLine "Der blev fundet GI assemblies i runtime output."

    foreach ($item in $giAssemblies) {
        $relative = Get-RelativePathSafe -BasePath $resolvedRuntimeOutputPath -TargetPath $item.FullName
        Write-WarnLine "GI artifact: $relative"
    }

    if ($FailOnGiArtifacts) {
        throw "GI runtime artifact verification fejlede: dk.gi artifacts fundet."
    }
}
else {
    Write-Info "GI runtime artifact verification bestået: ingen dk.gi artifacts fundet."
}
