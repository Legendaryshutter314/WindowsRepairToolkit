# Windows Repair Toolkit - Production Packaging

This repository contains two deployment paths.

## Recommended: Inno Setup

Inno Setup is the default/recommended installer for this utility because the application intentionally requires administrator rights and performs classic desktop/system-maintenance operations.

Prerequisites:
- Windows 10/11 x64
- .NET 8 SDK
- Inno Setup 6

Build:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Scripts\build-inno.ps1 -Version 1.0.0
```

Output:

```text
artifacts\installer\WindowsRepairToolkit-Setup-1.0.0-x64.exe
```

The installer:
- publishes a self-contained x64 .NET build;
- installs under Program Files;
- requests administrative privileges;
- creates a Start Menu shortcut;
- optionally creates a Desktop shortcut;
- registers a normal Windows uninstaller;
- launches the app after setup if selected.

## MSIX

The MSIX manifest declares both `runFullTrust` and `allowElevation`. The application executable itself retains its `requireAdministrator` Win32 manifest.

Prerequisites:
- Windows 10/11 x64
- .NET 8 SDK
- Windows 10/11 SDK (MakeAppx + SignTool)
- a code-signing certificate whose Subject exactly matches the Package Publisher

Development certificate example:

```powershell
.\Scripts\create-msix-dev-cert.ps1 `
  -Publisher "CN=WindowsRepairToolkit" `
  -Password "change-this-password"
```

Build and sign:

```powershell
.\Scripts\build-msix.ps1 `
  -Version 1.0.0.0 `
  -IdentityName "JK.WindowsRepairToolkit" `
  -Publisher "CN=WindowsRepairToolkit" `
  -PfxPath ".\artifacts\cert\WindowsRepairToolkit-Dev.pfx" `
  -PfxPassword "change-this-password"
```

Output:

```text
artifacts\msix\WindowsRepairToolkit-1.0.0.0-x64.msix
```

For production distribution, replace the development certificate with your organization/code-signing identity. Do not distribute a PFX private key with the app.

### Important MSIX elevation note

`allowElevation` is a restricted MSIX capability. It is suitable for enterprise/sideloaded scenarios when properly signed and trusted. Microsoft Store submissions that request this capability are subject to approval and may not be accepted for a general-purpose system repair utility.

## Build both

```powershell
.\Scripts\build-all-installers.ps1 `
  -Version 1.0.0 `
  -PfxPath ".\artifacts\cert\WindowsRepairToolkit-Dev.pfx" `
  -PfxPassword "change-this-password"
```

If you omit `-PfxPath`, the script can still create the MSIX file, but it will be unsigned and not normally installable until signed.

## Signing the Inno installer

The Inno configuration does not embed a private signing key. For public distribution, configure Inno Setup's `SignTool` directive or sign the resulting installer with SignTool/Azure Artifact Signing in your release pipeline.

## Signed production release

For production Authenticode signing and release metadata, see `RELEASE-SIGNING.md`.

The automated release pipeline is:

```powershell
.\Scripts\build-signed-release.ps1 -Version 1.0.0 -Publisher "JK" -CertificateThumbprint "YOUR_CERTIFICATE_THUMBPRINT"
```

This publishes the self-contained x64 app, signs the application executable, builds the Inno Setup package, signs the installer, verifies signatures, and emits a SHA-256 checksum plus `release-manifest.json` under `artifacts\release\<version>`.
