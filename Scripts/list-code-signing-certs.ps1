$ErrorActionPreference = 'Stop'
$oid = '1.3.6.1.5.5.7.3.3'

Get-ChildItem Cert:\CurrentUser\My, Cert:\LocalMachine\My -CodeSigningCert -ErrorAction SilentlyContinue |
    Where-Object { $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date) } |
    Sort-Object NotAfter -Descending |
    Select-Object Subject, Thumbprint, NotAfter, HasPrivateKey, PSParentPath |
    Format-Table -AutoSize
