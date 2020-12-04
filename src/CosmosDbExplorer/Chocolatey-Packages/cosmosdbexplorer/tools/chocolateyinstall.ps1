$ErrorActionPreference = 'Stop';

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url          = 'https://github.com/sachabruttin/CosmosDbExplorer/releases/download/0.8.0-beta/CosmosDbExplorer.0.8.0.zip'
$packageName  = $env:ChocolateyPackageName

Install-ChocolateyZipPackage $packageName $url $toolsDir

$softwareName = "CosmosDb Explorer"
$exePath = $toolsDir + "\cosmosdbexplorer.exe"
$workingDirectory = $unzipLocation + "\$zipName"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "$softwareName.lnk"

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath `
    -workingDirectory $workingDirectory
