# Windows Repair Toolkit — Release Metadata and Code Signing

This project now includes a production-oriented Authenticode release flow for the self-contained x64 Windows build and the Inno Setup installer.

## What is signed

The release script signs:

1. `artifacts\publish\win-x64\WindowsRepairToolkit.exe`
2. `artifacts\installer\WindowsRepairToolkit-Setup-<version>-x64.exe`

The signed application executable is embedded into the installer, so installed launches retain the Authenticode signature used by Windows/UAC.

## Release metadata

`WindowsRepairToolkit.csproj` contains default Win32/.NET version-resource metadata:

- Product: `Windows Repair Toolkit`
- Company/Author: `JK`
- Description: `Windows system health, repair, cleanup, and performance maintenance.`
- Copyright: `Copyright © 2026 JK`
- Version defaults: `1.0.0` / `1.0.0.0`

The publish and installer scripts accept overrides, so replace `JK` with your legal publisher/company name before a public release if appropriate.

The Inno Setup executable also receives matching Product, Company, Description, ProductVersion and Copyright version resources.

## Production certificate choices

For distribution outside your own machines, use a publicly trusted Authenticode/code-signing certificate or another trusted signing service. A self-signed certificate is useful for development/internal testing only and will not establish public trust or SmartScreen reputation by itself.

The scripts support either:

- A code-signing certificate/private key available through the Windows certificate store, selected by thumbprint. This is the preferred mode and works with many hardware-backed/provider-managed keys.
- An exportable `.pfx` certificate. The password is requested securely if it is not supplied as a `SecureString`.

Do not commit PFX files, private keys, passwords, or signing-service credentials to source control.

## Find an installed production certificate

```powershell
.\Scripts\list-code-signing-certs.ps1
```

Copy the desired `Thumbprint` value.

## Build a signed production release from the certificate store

```powershell
.\Scripts\build-signed-release.ps1 `
    -Version 1.0.0 `
    -Publisher "JK" `
    -CertificateThumbprint "YOUR_CERTIFICATE_THUMBPRINT"
```

If the certificate is installed in the Local Machine certificate store:

```powershell
.\Scripts\build-signed-release.ps1 `
    -Version 1.0.0 `
    -Publisher "JK" `
    -CertificateThumbprint "YOUR_CERTIFICATE_THUMBPRINT" `
    -MachineStore
```

## Build a signed release from a PFX

```powershell
$password = Read-Host "PFX password" -AsSecureString

.\Scripts\build-signed-release.ps1 `
    -Version 1.0.0 `
    -Publisher "JK" `
    -PfxPath "C:\Secure\Your-Code-Signing-Certificate.pfx" `
    -PfxPassword $password
```

If `-PfxPassword` is omitted, the signing script prompts for it.

## Outputs

A successful signed release is staged under:

```text
artifacts\release\1.0.0\
```

It contains:

```text
WindowsRepairToolkit-Setup-1.0.0-x64.exe
SHA256SUMS.txt
release-manifest.json
```

`release-manifest.json` records product/version metadata, build target, signer subject, Authenticode status, file size, and SHA-256 hash.

## Verify signatures manually

```powershell
.\Scripts\verify-signatures.ps1 `
    ".\artifacts\publish\win-x64\WindowsRepairToolkit.exe", `
    ".\artifacts\installer\WindowsRepairToolkit-Setup-1.0.0-x64.exe"
```

You can also inspect them through Windows:

1. Right-click the EXE.
2. Open **Properties**.
3. Open **Digital Signatures**.
4. Confirm the signature reports as valid and the expected publisher is shown.

## Development-only certificate

To exercise the signing pipeline without purchasing/connecting a production certificate:

```powershell
.\Scripts\create-dev-code-signing-cert.ps1
```

This creates a self-signed code-signing certificate in the current user's certificate store and exports a PFX/CER under `artifacts\cert`.

The script prints its thumbprint, which can then be used with:

```powershell
.\Scripts\build-signed-release.ps1 `
    -Version 1.0.0 `
    -CertificateThumbprint "THUMBPRINT_FROM_PREVIOUS_COMMAND"
```

The development certificate is not publicly trusted. Use it only for local/internal validation.

## Timestamping

The signing scripts use SHA-256 Authenticode signatures and an RFC 3161 timestamp URL. Timestamping lets a signature remain valid after the signing certificate itself expires, provided the signature was valid at signing time.

The default URL is configurable:

```powershell
.\Scripts\build-signed-release.ps1 `
    -Version 1.0.0 `
    -CertificateThumbprint "..." `
    -TimestampUrl "https://your-ca.example/rfc3161"
```

For production, use the timestamp endpoint recommended by the issuer of your code-signing certificate.

## Recommended release checklist

1. Set the final semantic version.
2. Use the exact legal publisher name associated with the production certificate.
3. Build with `build-signed-release.ps1`.
4. Confirm both Authenticode signatures are `Valid`.
5. Verify the SHA-256 hash from `SHA256SUMS.txt`.
6. Install on a clean Windows 11 VM.
7. Verify UAC shows the expected verified publisher.
8. Test install, upgrade, launch, core repair operations, and uninstall.
9. Publish the setup EXE together with its SHA-256 checksum and release notes.
