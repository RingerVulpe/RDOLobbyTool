<#
.SYNOPSIS
  Fetches the newest `vX.Y.Z-beta` release ZIP from GitHub,
  waits for the running app to quit, then extracts it over the install.
#>

param(
    [string]$repoOwner = "RingerVulpe",
    [string]$repoName  = "RDOLobbyTool",
    [string]$installDir= (Split-Path -Parent $MyInvocation.MyCommand.Path),
    [int]   $pid       = $null
)

# If we got a PID, wait for that process to die
if ($pid) {
    Write-Host "Waiting for process $pid to exit..."
    while (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
        Start-Sleep -Milliseconds 200
    }
    Write-Host "Process exited. Proceeding with update."
}

$apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases"
$headers = @{ 'User-Agent' = 'RDOL-Update-Script' }

Write-Host "→ Retrieving all releases from $repoOwner/$repoName..."
$allReleases = Invoke-RestMethod -Uri $apiUrl -Headers $headers

# pick only tags like v1.2.3-beta
$betaReleases = $allReleases `
  | Where-Object { $_.tag_name -match '^v\d+\.\d+\.\d+\-beta$' } `
  | Sort-Object published_at -Descending

if (-not $betaReleases) {
    Write-Error "No beta releases (vX.Y.Z-beta) found!"
    exit 1
}

$latest   = $betaReleases[0]
$tag      = $latest.tag_name
Write-Host "→ Latest beta tag: $tag"

# grab the .zip asset
$zipAsset = $latest.assets | Where-Object { $_.name -like '*.zip' } | Select-Object -First 1
if (-not $zipAsset) {
    Write-Error "No .zip asset in release $tag"
    exit 1
}

$downloadUrl = $zipAsset.browser_download_url
$tempZip     = Join-Path $env:TEMP "$repoName-$tag.zip"
$tempDir     = Join-Path $env:TEMP "$repoName-$tag"

Write-Host "→ Downloading $($zipAsset.name)…"
Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZip -Headers $headers

Write-Host "→ Extracting to $tempDir…"
if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
Expand-Archive -Path $tempZip -DestinationPath $tempDir

# source folder is the single subdirectory inside $tempDir
$src = Get-ChildItem -Path $tempDir | Where-Object PSIsContainer | Select-Object -First 1

Write-Host "→ Mirroring files into $installDir…"
Robocopy $src.FullName $installDir /MIR /NFL /NDL /NJH /NJS

Write-Host "→ Cleaning up…"
Remove-Item $tempZip -Force
Remove-Item $tempDir  -Recurse -Force

Write-Host "✅ Update complete! You’re now on $tag."
