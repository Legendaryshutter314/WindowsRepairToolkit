param(
    [Parameter(Mandatory=$true)][string]$ToolName
)

$command = Get-Command $ToolName -ErrorAction SilentlyContinue
if ($command -and $command.Source -and (Test-Path -LiteralPath $command.Source)) {
    return $command.Source
}

$kitsRoot = if (${env:ProgramFiles(x86)}) {
    Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\bin'
} else { $null }

if (-not $kitsRoot -or -not (Test-Path -LiteralPath $kitsRoot)) {
    throw "Windows 10/11 SDK was not found. Install the Windows SDK or put $ToolName on PATH."
}

$tool = Get-ChildItem -LiteralPath $kitsRoot -Directory -ErrorAction SilentlyContinue |
    Sort-Object Name -Descending |
    ForEach-Object { Join-Path $_.FullName "x64\$ToolName" } |
    Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
    Select-Object -First 1

if (-not $tool) { throw "$ToolName was not found in the installed Windows SDK." }
$tool
