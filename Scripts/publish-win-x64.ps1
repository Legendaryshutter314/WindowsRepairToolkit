[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [string]$Publisher = "JK",
    [string]$ProductName = "Windows Repair Toolkit",
    [string]$Description = "Windows system health, repair, cleanup, and performance maintenance.",
    [string]$Copyright = "Copyright © 2026 JK"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "WindowsRepairToolkit.csproj"
$out = Join-Path $root "artifacts\publish\win-x64"

if ($Version -notmatch '^(\d+)\.(\d+)\.(\d+)(?:[-+].*)?$') {
    throw "Version must begin with three numeric components, for example 1.0.0 or 1.0.0-beta.1."
}

$fileVersion = "$($Matches[1]).$($Matches[2]).$($Matches[3]).0"
$assemblyVersion = $fileVersion

if (Test-Path $out) { Remove-Item $out -Recurse -Force }
New-Item -ItemType Directory -Force -Path $out | Out-Null

Write-Host "Publishing $ProductName $Version (win-x64, self-contained)..." -ForegroundColor Cyan

# Do not pass text metadata with spaces as -p:Name=Value native arguments.
# dotnet/MSBuild can reparse those values and split them into separate switches.
# Environment variables are imported as MSBuild properties and preserve the text exactly.
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
        '-p:PublishSingleFile=false',
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

if ($publishExitCode -ne 0) { throw "dotnet publish failed with exit code $publishExitCode." }
Write-Host "Publish output: $out" -ForegroundColor Green
