Param
(

    [parameter(Mandatory = $true)]
    [string]
    $Version
)

New-Item -Path "." -Name "bin" -ItemType Directory -Force

# download the files and pack them
@{file = "CosmosDbExplorer.zip"; name = "CosmosDbExplorer.zip" } | ForEach-Object -Process {
    $download = "https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v$Version/$($_.file)" 
    Invoke-WebRequest $download -Out "./bin/$($_.name)"
    Expand-Archive -LiteralPath "./bin/$($_.name)" -DestinationPath "./bin/files" -Force
}

$content = Get-Content './setup.iss' -Raw
$content = $content.Replace('<VERSION>', $Version)
$ISSName = "./cosmosdbexplorer-$Version.iss"
$content | Out-File -Encoding 'UTF8' $ISSName
# package content
$installer = "install"
ISCC.exe /F$installer $ISSName
# get hash
$zipHash = Get-FileHash "Output/$installer.exe" -Algorithm SHA256
$zipHash.Hash | Out-File -Encoding 'UTF8' "Output/$installer.exe.sha256"