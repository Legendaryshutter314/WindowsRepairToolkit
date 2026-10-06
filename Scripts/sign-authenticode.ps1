[CmdletBinding(DefaultParameterSetName='Store')]
param(
    [Parameter(Mandatory=$true, Position=0)]
    [string[]]$Path,

    [Parameter(Mandatory=$true, ParameterSetName='Store')]
    [string]$CertificateThumbprint,

    [Parameter(ParameterSetName='Store')]
    [switch]$MachineStore,

    [Parameter(Mandatory=$true, ParameterSetName='Pfx')]
    [string]$PfxPath,

    [Parameter(ParameterSetName='Pfx')]
    [SecureString]$PfxPassword,

    [string]$TimestampUrl = 'http://timestamp.digicert.com'
)

$ErrorActionPreference = 'Stop'
$signTool = & (Join-Path $PSScriptRoot 'find-windows-sdk-tool.ps1') -ToolName 'signtool.exe'
if (-not $signTool) { throw 'signtool.exe could not be resolved.' }

function ConvertTo-PlainText {
    param([Parameter(Mandatory=$true)][SecureString]$Secure)
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Secure)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

$resolved = foreach ($item in $Path) {
    if (-not (Test-Path -LiteralPath $item -PathType Leaf)) { throw "File not found: $item" }
    (Resolve-Path -LiteralPath $item).Path
}

$plainPassword = $null
try {
    if ($PSCmdlet.ParameterSetName -eq 'Pfx') {
        if (-not (Test-Path -LiteralPath $PfxPath -PathType Leaf)) { throw "PFX file not found: $PfxPath" }
        $PfxPath = (Resolve-Path -LiteralPath $PfxPath).Path
        if (-not $PfxPassword) {
            $PfxPassword = Read-Host 'PFX password' -AsSecureString
        }
        $plainPassword = ConvertTo-PlainText -Secure $PfxPassword
    }

    foreach ($file in $resolved) {
        Write-Host "Signing: $file" -ForegroundColor Cyan
        $args = @('sign', '/fd', 'SHA256', '/tr', $TimestampUrl, '/td', 'SHA256')

        if ($PSCmdlet.ParameterSetName -eq 'Store') {
            if ($MachineStore) { $args += '/sm' }
            $args += @('/sha1', ($CertificateThumbprint -replace '\s',''))
        }
        else {
            $args += @('/f', $PfxPath, '/p', $plainPassword)
        }

        $args += $file
        & $signTool @args
        if ($LASTEXITCODE -ne 0) { throw "Signing failed for $file with exit code $LASTEXITCODE." }

        & $signTool verify /pa /all $file
        if ($LASTEXITCODE -ne 0) { throw "Signature verification failed for $file." }
    }
}
finally {
    $plainPassword = $null
}
