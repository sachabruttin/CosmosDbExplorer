# IMPORTANT: Before releasing this package, copy/paste the next 2 lines into PowerShell to remove all comments from this file:
#   $f='c:\path\to\thisFile.ps1'
#   gc $f | ? {$_ -notmatch "^\s*#"} | % {$_ -replace '(^.*?)\s*?[^``]#.*','$1'} | Out-File $f+".~" -en utf8; mv -fo $f+".~" $f

# 1. See the _TODO.md that is generated top level and read through that
# 2. Follow the documentation below to learn how to create a package for the package type you are creating.
# 3. In Chocolatey scripts, ALWAYS use absolute paths - $toolsDir gets you to the package's tools directory.
$ErrorActionPreference = 'Stop'; # stop on all errors
#Items that could be replaced based on what you call chocopkgup.exe with
#{{PackageName}} - Package Name (should be same as nuspec file and folder) |/p
#{{PackageVersion}} - The updated version | /v
#{{DownloadUrl}} - The url for the native file | /u
#{{PackageFilePath}} - Downloaded file if including it in package | /pp
#{{PackageGuid}} - This will be used later | /pg
#{{DownloadUrlx64}} - The 64-bit url for the native file | /u64
#{{Checksum}} - The checksum for the url | /c
#{{Checksumx64}} - The checksum for the 64-bit url | /c64
#{{ChecksumType}} - The checksum type for the url | /ct
#{{ChecksumTypex64}} - The checksum type for the 64-bit url | /ct64

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
# Internal packages (organizations) or software that has redistribution rights (community repo)
# - Use `Install-ChocolateyInstallPackage` instead of `Install-ChocolateyPackage`
#   and put the binaries directly into the tools folder (we call it embedding)
#$fileLocation = Join-Path $toolsDir 'NAME_OF_EMBEDDED_INSTALLER_FILE'
# If embedding binaries increase total nupkg size to over 1GB, use share location or download from urls
#$fileLocation = '\\SHARE_LOCATION\to\INSTALLER_FILE'
# Community Repo: Use official urls for non-redist binaries or redist where total package size is over 200MB
# Internal/Organization: Download from internal location (internet sources are unreliable)
$url          = 'https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v0.7.2-beta/CosmosDbExplorer.0.7.2.zip' # download url, HTTPS preferred
$packageName  = $env:ChocolateyPackageName

## Download and unpack a zip file - https://chocolatey.org/docs/helpers-install-chocolatey-zip-package
Install-ChocolateyZipPackage $packageName $url $toolsDir

## Install Desktop Icon
$softwareName = "CosmosDb Explorer"
$exePath = $toolsDir + "\cosmosdbexplorer.exe"
$workingDirectory = $unzipLocation + "\$zipName"
$desktop = $([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::CommonDesktopDirectory))
$desktopLink = Join-Path $desktop "$softwareName.lnk"

Install-ChocolateyShortcut `
    -shortcutFilePath $desktopLink `
    -targetPath $exePath `
    -workingDirectory $workingDirectory