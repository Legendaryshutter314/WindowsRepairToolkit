# Windows Repair Toolkit — Portable Build

The portable edition requires no installation and no separately installed .NET runtime.
It publishes as a self-contained x64 single-file executable and packages it in a ZIP.

## Build

```powershell
.\Scripts\build-portable.ps1 -Version 1.0.0
```

Output:

```text
artifacts\portable\WindowsRepairToolkit-Portable-1.0.0-x64.zip
artifacts\portable\WindowsRepairToolkit-Portable-1.0.0-x64.zip.sha256.txt
artifacts\portable\WindowsRepairToolkit-Portable-1.0.0-x64.manifest.json
```

## Signed portable build

```powershell
.\Scripts\build-portable.ps1 `
    -Version 1.0.0 `
    -CertificateThumbprint "YOUR_CERTIFICATE_THUMBPRINT"
```

Use `-MachineStore` when the selected code-signing certificate is in the Local Machine store.

## Portable data

The generated package contains `portable.flag`. When that marker is present, application-owned
persistent state stays beside the executable:

```text
WindowsRepairToolkit.exe
portable.flag
PORTABLE-README.txt
Data\
  Logs\
  StartupBackup\
```

If the portable media is unexpectedly read-only, startup/activity logs can fall back to `%TEMP%`
so diagnostics are not silently lost. Startup backup state does not intentionally fall back to an
installed-data location while portable mode is active.

## Important distinction

"Portable" describes deployment and the toolkit's own application data. The repair actions are
supposed to change the Windows installation on which the toolkit runs. DISM, SFC, Windows Update,
Search-index rebuilding, startup changes, cleanup, and AppX removal are therefore not sandboxed.
