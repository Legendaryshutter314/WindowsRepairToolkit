[CmdletBinding()]
param(
    [string]$Version = "1.0.0",
    [string]$Publisher = "JK",
    [string]$ProductName = "Windows Repair Toolkit",
    [string]$Description = "Windows system health, repair, cleanup, and performance maintenance.",
    [string]$Copyright = "Copyright © 2026 JK",
    [switch]$SkipPublish,
    [string]$IsccPath
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Inno release version must be numeric MAJOR.MINOR.PATCH, for example 1.0.0."
}

if (-not $SkipPublish) {
    & (Join-Path $PSScriptRoot "publish-win-x64.ps1") `
        -Version $Version `
        -Publisher $Publisher `
        -ProductName $ProductName `
        -Description $Description `
        -Copyright $Copyright
    if ($LASTEXITCODE -ne 0) { throw "Publish failed." }
}

function Resolve-IsccPath {
    param([string]$ExplicitPath)

    if ($ExplicitPath) {
        if (Test-Path -LiteralPath $ExplicitPath -PathType Leaf) {
            return (Resolve-Path -LiteralPath $ExplicitPath).Path
        }
        throw "The supplied ISCC path does not exist: $ExplicitPath"
    }

    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($command -and $command.Source -and (Test-Path -LiteralPath $command.Source)) {
        return $command.Source
    }

    $candidatePaths = @()
    if (${env:ProgramFiles(x86)}) {
        $candidatePaths += Join-Path ${env:ProgramFiles(x86)} "Inno Setup 7\ISCC.exe"
        $candidatePaths += Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
    }
    if ($env:ProgramFiles) {
        $candidatePaths += Join-Path $env:ProgramFiles "Inno Setup 7\ISCC.exe"
        $candidatePaths += Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"
    }
    if ($env:LOCALAPPDATA) {
        $candidatePaths += Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 7\ISCC.exe"
        $candidatePaths += Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
    }

    foreach ($candidate in $candidatePaths | Select-Object -Unique) {
        if ($candidate -and (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    $registryRoots = @(
        "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )
    foreach ($registryRoot in $registryRoots) {
        $entries = Get-ItemProperty $registryRoot -ErrorAction SilentlyContinue |
            Where-Object { $_.DisplayName -like "Inno Setup*" }
        foreach ($entry in $entries) {
            if ($entry.InstallLocation) {
                $candidate = Join-Path $entry.InstallLocation "ISCC.exe"
                if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                    return (Resolve-Path -LiteralPath $candidate).Path
                }
            }
        }
    }
    return $null
}

$iscc = Resolve-IsccPath -ExplicitPath $IsccPath
if (-not $iscc) {
    throw @"
Inno Setup compiler (ISCC.exe) was not found.
Checked PATH, machine-wide Program Files locations, per-user LocalAppData locations, and common registry entries for Inno Setup 6/7.
You can also pass it explicitly with -IsccPath.
"@
}

$iss = Join-Path $root "Installer\InnoSetup\WindowsRepairToolkit.iss"
$publishDir = Join-Path $root "artifacts\publish\win-x64"
$out = Join-Path $root "artifacts\installer"

if (-not (Test-Path -LiteralPath $iss -PathType Leaf)) { throw "Inno Setup script was not found: $iss" }
if (-not (Test-Path -LiteralPath $publishDir -PathType Container)) { throw "Publish directory was not found: $publishDir. Run without -SkipPublish first." }
New-Item -ItemType Directory -Force -Path $out | Out-Null

Write-Host "Using Inno Setup compiler: $iscc" -ForegroundColor DarkGray
Write-Host "Building Inno Setup installer..." -ForegroundColor Cyan

& $iscc `
    "/DMyAppVersion=$Version" `
    "/DMyAppPublisher=$Publisher" `
    "/DMyAppName=$ProductName" `
    "/DMyAppDescription=$Description" `
    "/DMyAppCopyright=$Copyright" `
    "/DPublishDir=$publishDir" `
    "/DOutputDir=$out" `
    $iss

if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed with exit code $LASTEXITCODE." }
Write-Host "Installer output: $out" -ForegroundColor Green
