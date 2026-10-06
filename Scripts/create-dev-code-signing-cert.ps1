[CmdletBinding()]
param(
    [string]$Subject = 'CN=JK Windows Repair Toolkit Development',
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'artifacts\cert' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$cert = New-SelfSignedCertificate `
    -Type CodeSigningCert `
    -Subject $Subject `
    -CertStoreLocation 'Cert:\CurrentUser\My' `
    -KeyExportPolicy Exportable `
    -KeyLength 3072 `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(2)

$password = Read-Host 'Choose a password for the development PFX' -AsSecureString
$pfx = Join-Path $OutputDirectory 'WindowsRepairToolkit-Development-CodeSigning.pfx'
$cer = Join-Path $OutputDirectory 'WindowsRepairToolkit-Development-CodeSigning.cer'

Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $password | Out-Null
Export-Certificate -Cert $cert -FilePath $cer | Out-Null

Write-Host 'Development code-signing certificate created.' -ForegroundColor Green
Write-Host "Thumbprint: $($cert.Thumbprint)"
Write-Host "PFX:        $pfx"
Write-Host "CER:        $cer"
Write-Warning 'This self-signed certificate is for development/internal testing. It is not publicly trusted and will not by itself remove SmartScreen warnings on other PCs.'
