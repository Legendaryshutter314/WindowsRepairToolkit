[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [string]$Publisher = "JK",
    [string]$ProductName = "Windows Repair Toolkit",
    [string]$Description = "Windows system health, repair, cleanup, and performance maintenance.",
    [string]$Copyright = "Copyright © 2026 JK",
    [string]$CertificateThumbprint,
    [switch]$MachineStore
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "WindowsRepairToolkit.csproj"
$artifactRoot = Join-Path $root "artifacts\portable"
$folderName = "WindowsRepairToolkit-Portable-$Version-x64"
$out = Join-Path $artifactRoot $folderName
$zip = Join-Path $artifactRoot "$folderName.zip"

if ($Version -notmatch '^(\d+)\.(\d+)\.(\d+)(?:[-+].*)?$') {
    throw "Version must begin with three numeric components, for example 1.0.0 or 1.0.0-beta.1."
}

$fileVersion = "$($Matches[1]).$($Matches[2]).$($Matches[3]).0"
$assemblyVersion = $fileVersion

if (Test-Path $out) { Remove-Item $out -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }
New-Item -ItemType Directory -Force -Path $out | Out-Null

Write-Host "Publishing $ProductName $Version as a portable self-contained x64 build..." -ForegroundColor Cyan

$metadataEnvironment = @{
    WrtProduct     = $ProductName
    WrtPublisher   = $Publisher
    WrtDescription = $Description
    WrtCopyright   = $Copyright
}

$previousEnvironment = @{}
foreach ($name in $metadataEnvironment.Keys) {
    $previousEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process')
    [Environment]::SetEnvironmentVariable($name, [string]$metadataEnvironment[$name], 'Process')
}

try {
    $publishArgs = @(
        'publish', $project,
        '-c', $Configuration,
        '-r', 'win-x64',
        '--self-contained', 'true',
        '-o', $out,
        "-p:Version=$Version",
        "-p:FileVersion=$fileVersion",
        "-p:AssemblyVersion=$assemblyVersion",
        "-p:InformationalVersion=$Version",
        '-p:PublishSingleFile=true',
        '-p:IncludeNativeLibrariesForSelfExtract=true',
        '-p:EnableCompressionInSingleFile=true',
        '-p:PublishTrimmed=false',
        '-p:DebugType=None',
        '-p:DebugSymbols=false'
    )

    & dotnet @publishArgs
    $publishExitCode = $LASTEXITCODE
}
finally {
    foreach ($name in $metadataEnvironment.Keys) {
        [Environment]::SetEnvironmentVariable($name, $previousEnvironment[$name], 'Process')
    }
}

if ($publishExitCode -ne 0) {
    throw "dotnet publish failed with exit code $publishExitCode."
}

$exe = Join-Path $out 'WindowsRepairToolkit.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Portable executable was not produced: $exe"
}

# This marker switches application-owned persistence to .\Data beside the EXE.
Set-Content -LiteralPath (Join-Path $out 'portable.flag') -Value 'Windows Repair Toolkit portable mode' -Encoding UTF8

$readme = @"
Windows Repair Toolkit $Version - Portable x64

No installation is required.

1. Extract this ZIP to a writable local folder or USB drive.
2. Run WindowsRepairToolkit.exe.
3. Approve the Windows UAC elevation prompt.

Portable application data:
  .\Data\Logs
  .\Data\StartupBackup

The toolkit itself is portable, but repair operations intentionally modify the
Windows computer you run them on (for example DISM/SFC repairs, Windows Update,
startup configuration, AppX packages, Search index data, and temporary files).

Do not run directly from inside the ZIP file. Extract it first.
"@
Set-Content -LiteralPath (Join-Path $out 'PORTABLE-README.txt') -Value $readme -Encoding UTF8

if ($CertificateThumbprint) {
    Write-Host "Signing portable executable..." -ForegroundColor Cyan
    $signScript = Join-Path $PSScriptRoot 'sign-authenticode.ps1'
    & $signScript -Path $exe -CertificateThumbprint $CertificateThumbprint -MachineStore:$MachineStore
}

Write-Host "Creating portable ZIP..." -ForegroundColor Cyan
Compress-Archive -Path (Join-Path $out '*') -DestinationPath $zip -CompressionLevel Optimal -Force

$hash = Get-FileHash -LiteralPath $zip -Algorithm SHA256
$hashPath = "$zip.sha256.txt"
Set-Content -LiteralPath $hashPath -Value "$($hash.Hash)  $([IO.Path]::GetFileName($zip))" -Encoding ASCII

$manifest = [ordered]@{
    product = $ProductName
    version = $Version
    architecture = 'x64'
    packageType = 'portable-zip'
    selfContained = $true
    singleFile = $true
    portableData = '.\\Data'
    file = [IO.Path]::GetFileName($zip)
    sha256 = $hash.Hash
    signed = [bool]$CertificateThumbprint
    createdUtc = [DateTime]::UtcNow.ToString('o')
}
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $artifactRoot "$folderName.manifest.json") -Encoding UTF8

Write-Host "Portable package created:" -ForegroundColor Green
Write-Host "  $zip"
Write-Host "SHA-256:" -ForegroundColor Green
Write-Host "  $($hash.Hash)"
