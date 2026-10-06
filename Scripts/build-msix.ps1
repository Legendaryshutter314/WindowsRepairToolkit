[CmdletBinding()]
param(
    [string]$Version = "1.0.0.0",
    [string]$IdentityName = "JK.WindowsRepairToolkit",
    [string]$Publisher = "CN=WindowsRepairToolkit",
    [string]$PublisherDisplayName = "JK",
    [string]$ProductName = "Windows Repair Toolkit",
    [string]$Description = "Windows system health, repair, cleanup, and performance maintenance.",
    [string]$PfxPath,
    [SecureString]$PfxPassword,
    [string]$TimestampUrl = 'http://timestamp.digicert.com',
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "MSIX Version must have four numeric components, for example 1.0.0.0."
}

if (-not $SkipPublish) {
    $assemblyVersion = ($Version -split '\.')[0..2] -join '.'
    & (Join-Path $PSScriptRoot "publish-win-x64.ps1") `
        -Version $assemblyVersion `
        -Publisher $PublisherDisplayName `
        -ProductName $ProductName `
        -Description $Description
    if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
}

$publish = Join-Path $root "artifacts\publish\win-x64"
$stage = Join-Path $root "artifacts\msix-stage"
$outDir = Join-Path $root "artifacts\msix"
$msix = Join-Path $outDir "WindowsRepairToolkit-$Version-x64.msix"
$template = Join-Path $root "Installer\MSIX\Package.appxmanifest.template"
$assets = Join-Path $root "Installer\MSIX\Assets"

if (-not (Test-Path (Join-Path $publish "WindowsRepairToolkit.exe"))) {
    throw "Published executable not found at $publish. Run publish-win-x64.ps1 first."
}

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path $stage, $outDir | Out-Null
Copy-Item (Join-Path $publish '*') $stage -Recurse -Force
New-Item -ItemType Directory -Force -Path (Join-Path $stage 'Assets') | Out-Null
Copy-Item (Join-Path $assets '*') (Join-Path $stage 'Assets') -Force

$manifest = Get-Content $template -Raw
$manifest = $manifest.Replace('__IDENTITY_NAME__', $IdentityName)
$manifest = $manifest.Replace('__PUBLISHER__', $Publisher)
$manifest = $manifest.Replace('__VERSION__', $Version)
$manifest = $manifest.Replace('__DISPLAY_NAME__', $ProductName)
$manifest = $manifest.Replace('__PUBLISHER_DISPLAY_NAME__', $PublisherDisplayName)
$manifest = $manifest.Replace('__DESCRIPTION__', $Description)
Set-Content -Path (Join-Path $stage 'AppxManifest.xml') -Value $manifest -Encoding UTF8

$makeappx = & (Join-Path $PSScriptRoot 'find-windows-sdk-tool.ps1') -ToolName 'makeappx.exe'
Write-Host "Building MSIX..." -ForegroundColor Cyan
& $makeappx pack /d $stage /p $msix /o
if ($LASTEXITCODE -ne 0) { throw "MakeAppx failed with exit code $LASTEXITCODE." }

if ($PfxPath) {
    if (-not (Test-Path $PfxPath)) { throw "PFX not found: $PfxPath" }
    if (-not $PfxPassword) { $PfxPassword = Read-Host 'PFX password' -AsSecureString }

    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($PfxPassword)
    try {
        $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
        $signtool = & (Join-Path $PSScriptRoot 'find-windows-sdk-tool.ps1') -ToolName 'signtool.exe'
        Write-Host "Signing MSIX..." -ForegroundColor Cyan
        & $signtool sign /fd SHA256 /f $PfxPath /p $plain /tr $TimestampUrl /td SHA256 $msix
        if ($LASTEXITCODE -ne 0) { throw "SignTool failed with exit code $LASTEXITCODE." }
        & $signtool verify /pa /all $msix
        if ($LASTEXITCODE -ne 0) { throw "MSIX signature verification failed." }
        Write-Host "Signed MSIX: $msix" -ForegroundColor Green
    }
    finally {
        if ($bstr -ne [IntPtr]::Zero) { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
        $plain = $null
    }
} else {
    Write-Warning "MSIX was created but not signed. Windows requires MSIX packages to be signed before normal installation."
    Write-Host "Unsigned MSIX: $msix" -ForegroundColor Yellow
}
