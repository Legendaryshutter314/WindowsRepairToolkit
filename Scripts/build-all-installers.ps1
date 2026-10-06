[CmdletBinding()]
param(
    [string]$Version = "1.0.0",
    [string]$MsixIdentityName = "JK.WindowsRepairToolkit",
    [string]$MsixPublisher = "CN=WindowsRepairToolkit",
    [string]$PfxPath,
    [string]$PfxPassword
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "publish-win-x64.ps1") -Version $Version
& (Join-Path $PSScriptRoot "build-inno.ps1") -Version $Version -SkipPublish

$msixVersion = "$Version.0"
if ($Version -match '^\d+\.\d+\.\d+\.\d+$') { $msixVersion = $Version }
& (Join-Path $PSScriptRoot "build-msix.ps1") `
    -Version $msixVersion `
    -IdentityName $MsixIdentityName `
    -Publisher $MsixPublisher `
    -PfxPath $PfxPath `
    -PfxPassword $PfxPassword `
    -SkipPublish
