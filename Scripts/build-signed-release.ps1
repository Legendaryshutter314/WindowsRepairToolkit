[CmdletBinding(DefaultParameterSetName='Store')]
param(
    [string]$Version = '1.0.0',
    [string]$Publisher = 'JK',
    [string]$ProductName = 'Windows Repair Toolkit',
    [string]$Description = 'Windows system health, repair, cleanup, and performance maintenance.',
    [string]$Copyright = 'Copyright © 2026 JK',
    [string]$TimestampUrl = 'http://timestamp.digicert.com',
    [string]$IsccPath,

    [Parameter(Mandatory=$true, ParameterSetName='Store')]
    [string]$CertificateThumbprint,

    [Parameter(ParameterSetName='Store')]
    [switch]$MachineStore,

    [Parameter(Mandatory=$true, ParameterSetName='Pfx')]
    [string]$PfxPath,

    [Parameter(ParameterSetName='Pfx')]
    [SecureString]$PfxPassword
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Release version must be numeric MAJOR.MINOR.PATCH, for example 1.0.0."
}

$publishScript = Join-Path $PSScriptRoot 'publish-win-x64.ps1'
$signScript = Join-Path $PSScriptRoot 'sign-authenticode.ps1'
$innoScript = Join-Path $PSScriptRoot 'build-inno.ps1'

& $publishScript `
    -Version $Version `
    -Publisher $Publisher `
    -ProductName $ProductName `
    -Description $Description `
    -Copyright $Copyright

$appExe = Join-Path $root 'artifacts\publish\win-x64\WindowsRepairToolkit.exe'

$signArgs = @{
    Path = @($appExe)
    TimestampUrl = $TimestampUrl
}
if ($PSCmdlet.ParameterSetName -eq 'Store') {
    $signArgs.CertificateThumbprint = $CertificateThumbprint
    if ($MachineStore) { $signArgs.MachineStore = $true }
}
else {
    $signArgs.PfxPath = $PfxPath
    if ($PfxPassword) { $signArgs.PfxPassword = $PfxPassword }
}

& $signScript @signArgs

$buildArgs = @{
    Version = $Version
    Publisher = $Publisher
    ProductName = $ProductName
    Description = $Description
    Copyright = $Copyright
    SkipPublish = $true
}
if ($IsccPath) { $buildArgs.IsccPath = $IsccPath }
& $innoScript @buildArgs

$installer = Join-Path $root "artifacts\installer\WindowsRepairToolkit-Setup-$Version-x64.exe"
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) {
    throw "Expected installer was not produced: $installer"
}

$signArgs.Path = @($installer)
& $signScript @signArgs

$releaseDir = Join-Path $root "artifacts\release\$Version"
if (Test-Path -LiteralPath $releaseDir) { Remove-Item -LiteralPath $releaseDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

$releaseInstaller = Join-Path $releaseDir (Split-Path -Leaf $installer)
Copy-Item -LiteralPath $installer -Destination $releaseInstaller -Force

$hash = Get-FileHash -LiteralPath $releaseInstaller -Algorithm SHA256
$hashLine = "$($hash.Hash.ToLowerInvariant())  $(Split-Path -Leaf $releaseInstaller)"
Set-Content -LiteralPath (Join-Path $releaseDir 'SHA256SUMS.txt') -Value $hashLine -Encoding ascii

$appVersionInfo = (Get-Item -LiteralPath $appExe).VersionInfo
$appSignature = Get-AuthenticodeSignature -FilePath $appExe
$installerSignature = Get-AuthenticodeSignature -FilePath $releaseInstaller

$manifest = [ordered]@{
    product = $ProductName
    version = $Version
    publisher = $Publisher
    description = $Description
    architecture = 'x64'
    runtime = 'win-x64 self-contained .NET 8'
    minimum_windows_build = '19041'
    generated_utc = [DateTime]::UtcNow.ToString('o')
    executable = [ordered]@{
        file = 'WindowsRepairToolkit.exe'
        file_version = $appVersionInfo.FileVersion
        product_version = $appVersionInfo.ProductVersion
        signature_status = [string]$appSignature.Status
        signer = if ($appSignature.SignerCertificate) { $appSignature.SignerCertificate.Subject } else { $null }
    }
    installer = [ordered]@{
        file = (Split-Path -Leaf $releaseInstaller)
        sha256 = $hash.Hash.ToLowerInvariant()
        size_bytes = (Get-Item -LiteralPath $releaseInstaller).Length
        signature_status = [string]$installerSignature.Status
        signer = if ($installerSignature.SignerCertificate) { $installerSignature.SignerCertificate.Subject } else { $null }
    }
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $releaseDir 'release-manifest.json') -Encoding utf8

if ($appSignature.Status -ne 'Valid') { throw "Application signature is not valid: $($appSignature.Status)" }
if ($installerSignature.Status -ne 'Valid') { throw "Installer signature is not valid: $($installerSignature.Status)" }

Write-Host ''
Write-Host 'Signed release created successfully:' -ForegroundColor Green
Write-Host "  $releaseInstaller"
Write-Host "  $(Join-Path $releaseDir 'SHA256SUMS.txt')"
Write-Host "  $(Join-Path $releaseDir 'release-manifest.json')"
