
Param
(
    [parameter(Mandatory = $true)]
    [string]
    $Version,
    [parameter(Mandatory = $false)]
    [string]
    $Token
)

function Get-HashForArchitecture_old {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $Version
    )
    $output = "./output/CosmosDbExplorer.zip"
    $url = "https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v$Version/CosmosDbExplorer.zip"
    Invoke-WebRequest $url -Out $output
    $hash = Get-FileHash $output -Algorithm SHA256
    return $hash.Hash
}

function Get-HashForArchitecture {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $Version
    )
    $hash = (new-object Net.WebClient).DownloadString("https://github.com/sachabruttin/CosmosDbExplorer/releases/download/v$Version-beta/CosmosDbExplorer.zip.sha256")
    return $hash.Trim()
}


function Write-MetaData {
    param (
        [parameter(Mandatory = $true)]
        [string]
        $FileName,
        [parameter(Mandatory = $true)]
        [string]
        $Version,
        [parameter(Mandatory = $true)]
        [string]
        $Path,
        [parameter(Mandatory = $true)]
        [string]
        $Hash
    )

    $outPath = $Path -eq 'templates' ? "./output/$FileName" : "./output/$Path/$FileName" 
    $filePath = $Path -like 'templates' ? "./templates/$FileName" : "./templates/$Path/$FileName"

    $tagName = "v$Version"
    $Version -match  '(?<version>\d.\d.\d)(?:-)(?<beta>.*)'

    $content = Get-Content $filePath -Raw
    $content = $content.Replace('$version', $Matches.version)
    $content = $content.Replace('<tag-name>', $tagName)
    $content = $content.Replace('<HASH>', $Hash)
    $date = Get-Date -Format "yyyy-MM-dd"
    $content = $content.Replace('<DATE>', $date)
    $content | Out-File -Encoding 'UTF8' $outPath
}

New-Item -Path $PWD -Name "output/tools" -ItemType "directory"
# Get all files inside the folder and adjust the version/hash
$Hash = Get-HashForArchitecture  -Version $Version

Get-ChildItem './templates/*' -Recurse -File | ForEach-Object -Process {
    Write-MetaData -FileName $_.Name -Version $Version -Hash $Hash -Path $_.Directory.BaseName
}

# package
choco pack "./output/cosmosdbexplorer.nuspec" --out "./output" --allow-unofficial


if (-not $Token) {
    return
}

# push
$nupkg = (Get-ChildItem '*.nupkg' -Recurse -File).FullName
choco push $nupkg --api-key=$Token --allow-unofficial