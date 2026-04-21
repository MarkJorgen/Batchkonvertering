param (
    [Parameter(Mandatory = $true)]
    [string]$DirectoryPath,
    
    [Parameter(Mandatory = $true)]
    [string]$ZipFilePath,
    
    [Parameter(Mandatory = $false)]
    [System.IO.Compression.CompressionLevel]$CompressionLevel = [System.IO.Compression.CompressionLevel]::Optimal,
    
    [Parameter(Mandatory = $false)]
    [switch]$IncludeBaseDirectory
)

[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null

[System.IO.Compression.ZipFile]::CreateFromDirectory($DirectoryPath, $ZipFilePath, $CompressionLevel, $IncludeBaseDirectory)