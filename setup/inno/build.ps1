Param
(

    [parameter(Mandatory = $true)]
    [string]
    $Version
)

New-Item -Path "." -Name "bin" -ItemType Directory -Force

# download the files and pack them

$download = "https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v$Version/CosmosDbExplorer.zip" 
Invoke-WebRequest $download -Out ".\bin\CosmosDbExplorer.zip"
Expand-Archive -LiteralPath ".\bin\CosmosDbExplorer.zip" -DestinationPath ".\bin\files" -Force

$content = Get-Content './setup.iss' -Raw
$content = $content.Replace('<VERSION>', $Version)
$ISSName = ".\cosmosdbexplorer-$Version.iss"
$content | Out-File -Encoding 'UTF8' $ISSName
# package content
$installer = "install"
ISCC.exe /F$installer $ISSName
# get hash
$zipHash = Get-FileHash "Output\$installer.exe" -Algorithm SHA256
$zipHash.Hash | Out-File -Encoding 'UTF8' "Output\$installer.exe.sha256"
