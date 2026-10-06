[CmdletBinding()]
param(
    [string]$Publisher = "CN=WindowsRepairToolkit",
    [string]$PfxPath,
    [string]$Password = "WindowsRepairToolkit-Dev"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
if (-not $PfxPath) { $PfxPath = Join-Path $root "artifacts\cert\WindowsRepairToolkit-Dev.pfx" }
$certDir = Split-Path -Parent $PfxPath
New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject $Publisher `
    -KeyUsage DigitalSignature `
    -FriendlyName "Windows Repair Toolkit MSIX Development" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$secure = ConvertTo-SecureString $Password -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $PfxPath -Password $secure | Out-Null

Write-Host "Created development signing certificate:" -ForegroundColor Green
Write-Host "  Subject: $Publisher"
Write-Host "  PFX:     $PfxPath"
Write-Host ""
Write-Warning "This is a development certificate. Trust its public certificate only on test/managed devices; do not distribute the PFX or its password."
