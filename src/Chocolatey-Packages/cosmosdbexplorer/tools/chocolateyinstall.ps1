$ErrorActionPreference = 'Stop';

$toolsDir     = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url          = 'https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v0.9.0-beta/CosmosDbExplorer.zip'
$packageName  = $env:ChocolateyPackageName
$checksum     = "10D9EAA866851AA558E8F01AD6A1ED42D78BF9DF051D69A904099E27A10D1383"

Install-ChocolateyZipPackage `
    -PackageName $packageName `
    -Url $url `
    -UnzipLocation $toolsDir `
    -Checksum $checksum `
    -ChecksumType 'sha256'

$softwareName = "CosmosDb Explorer"
$exePath = $toolsDir + "\cosmosdbexplorer.exe"
$workingDirectory = $unzipLocation + "\$zipName"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "$softwareName.lnk"

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath `
    -workingDirectory $workingDirectory
