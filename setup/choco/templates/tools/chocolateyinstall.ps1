$ErrorActionPreference = 'Stop';

$toolsDir       = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url            = 'https://github.com/sachabruttin/CosmosDbExplorer/releases/download/<tag-name>/CosmosDbExplorer.zip'
$packageName    = $env:ChocolateyPackageName
$checksum       = "<HASH>"

Install-ChocolateyZipPackage `
    -PackageName $packageName `
    -Url $url `
    -UnzipLocation $toolsDir `
    -ChecksumType "sha256" `
    -Checksum $checksum

$softwareName = "CosmosDb Explorer"
$exePath = $toolsDir + "\cosmosdbexplorer.exe"
$workingDirectory = $unzipLocation + "\$zipName"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "$softwareName.lnk"

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath `
    -workingDirectory $workingDirectory
