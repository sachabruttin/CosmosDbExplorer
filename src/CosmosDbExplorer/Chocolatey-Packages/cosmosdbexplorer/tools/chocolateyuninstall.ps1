$ErrorActionPreference = 'Stop';

$packageName  = $env:ChocolateyPackageName

Uninstall-ChocolateyZipPackage `
  -PackageName $packageName `
  -ZipFileName 'CosmosDbExplorer.0.8.2.zip'

$softwareName = "CosmosDb Explorer"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "$softwareName.lnk"
Remove-Item $desktopLink -force -erroraction silentlycontinue
