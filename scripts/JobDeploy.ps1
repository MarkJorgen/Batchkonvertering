param(
    [Parameter(Mandatory = $true)]
    $ResourceGroupName,

    [Parameter(Mandatory = $true)]
    $WebappName,

    [Parameter(Mandatory = $true)]
    $WebjobName,

    [Parameter(Mandatory = $true)]
    $Path,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Continuous", "Triggered")]
    $ScheduleName = "Continuous",

    [Parameter(Mandatory = $false)]
    $SubscriptionId,

    [Parameter(Mandatory = $false)]
    [ValidateSet("zip", "folder")]
    $DeploymentType = "zip"
)

if (-not (Test-Path $Path)) 
{
    throw [System.IO.FileNotFoundException] "$($Path) not found."
}

Add-Type -assembly "System.IO.Compression.FileSystem"
$FilePathInZip = "settings.job"

$secureToken = (Get-AzAccessToken -AsSecureString).Token

$token = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken))
$authHeader = "Bearer $($token)"

$apiBaseUrl = "https://$($WebappName).scm.azurewebsites.net/api"

$WebjobName = $WebjobName -replace "\.", "-" # Webjob names cannot contain dots

if ($SubscriptionId) {
    Set-AzContext -SubscriptionId $SubscriptionId
}

$files = Get-ChildItem -Path $Path -Recurse

if ($DeploymentType -eq "zip") {
    $deployUrl = "$($apiBaseUrl)/$($ScheduleName)webjobs/$($WebjobName)"
    Write-Host "Uploading " $Path  " to " $deployUrl
    $ZipHeaders = @{
        Authorization = $authHeader
        "Content-Disposition" = "attachment; filename=$($files[0].Name)"
    }

    # Upload zip file to webjob
    Write-Host "Deploy to url: $($deployUrl)"
    $response = Invoke-WebRequest -Uri  $deployUrl -Headers $ZipHeaders -InFile $files[0] -ContentType "application/zip" -Method Put
} elseif ($DeploymentType -eq "folder") {
    foreach ($file in $files) {
        $deployUrl = "$($apiBaseUrl)/vfs/site/wwwroot/$($file.FullName.Substring($Path.Length + 1))"
        Write-Host "Uploading " $file.FullName  " to " $deployUrl
        $FileHeaders = @{
            Authorization = $authHeader
        }
        $response = Invoke-WebRequest -Uri  $deployUrl -Headers $FileHeaders -InFile $file.FullName -Method Put
    }
}

$settingsUrl = "$($apiBaseUrl)/$($ScheduleName)webjobs/$($WebjobName)/settings"

try {
    # Extract settings.job from the zip file
    $Zip = [System.IO.Compression.ZipFile]::OpenRead($files[0].FullName)
    $ZipEntries = [array]($Zip.Entries | where-object {
        return $_.FullName -eq $FilePathInZip
    })

    if (!$ZipEntries -or $ZipEntries.Length -lt 1) {
        throw "File ""$FilePathInZip"" couldn't be found in zip ""$ZipFilePath""."
    }

    if (!$ZipEntries -or $ZipEntries.Length -gt 1) {
        throw "More than one file ""$FilePathInZip"" found in zip ""$ZipFilePath""."
    }

    $ZipStream = $ZipEntries[0].Open()
    $Reader = [System.IO.StreamReader]::new($ZipStream)
    $settingsContent  = $Reader.ReadToEnd()
    if ($Reader) { $Reader.Dispose() }
    if ($Zip) { $Zip.Dispose() }

    # Prepare web request to upload settings.job
    $settingsHeaders = @{
        Authorization = $authHeader
        "Content-Type" = "application/json"
    }

    Write-Host "Settings content: " $settingsContent
    Write-Host "Uploading settings to " $settingsUrl
    Invoke-RestMethod -Uri $settingsUrl -Headers $settingsHeaders -Body $settingsContent -Method Put
} catch {
    Write-Error "An error occurred: $_"
}