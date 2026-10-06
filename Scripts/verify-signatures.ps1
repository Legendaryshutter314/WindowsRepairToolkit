[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [string[]]$Path
)

$ErrorActionPreference = 'Stop'
$signTool = & (Join-Path $PSScriptRoot 'find-windows-sdk-tool.ps1') -ToolName 'signtool.exe'
foreach ($item in $Path) {
    if (-not (Test-Path -LiteralPath $item -PathType Leaf)) { throw "File not found: $item" }
    $file = (Resolve-Path -LiteralPath $item).Path
    Write-Host "Verifying: $file" -ForegroundColor Cyan
    & $signTool verify /pa /all /v $file
    if ($LASTEXITCODE -ne 0) { throw "Signature verification failed for $file." }
}
Write-Host 'All signatures verified.' -ForegroundColor Green
