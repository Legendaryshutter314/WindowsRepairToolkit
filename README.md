# Windows Repair Toolkit — MVVM Edition

A Windows-only .NET 8 WPF utility for system repair, cleanup, startup management, profile analysis, Search/Explorer repair, drive optimization, debloat review, and Windows Update maintenance.

## Architecture

This edition uses MVVM rather than placing application behavior in `MainWindow.xaml.cs`.

```text
MainWindow.xaml (View)
        |
        | bindings / ICommand
        v
MainWindowViewModel
        |
        +-- SystemRepairViewModel
        +-- CleanupStorageViewModel
        +-- StartupViewModel
        +-- DebloatViewModel
        +-- ProfileViewModel
        +-- SearchExplorerViewModel
        +-- WindowsUpdateViewModel
        |
        v
OperationViewModel + Services
        |
        v
Repair Tasks / Windows APIs / PowerShell / native utilities
```

`MainWindow.xaml.cs` now contains only custom-window chrome behavior: dragging, minimize, maximize/restore, and close. Repair, cleanup, scanning, confirmation, progress, cancellation, logging, collections, and command state are outside the view.

### MVVM infrastructure

- `ViewModels/ObservableObject.cs` — `INotifyPropertyChanged` base class.
- `Commands/RelayCommand.cs` — synchronous `ICommand` implementation.
- `Commands/AsyncRelayCommand.cs` — asynchronous command implementation with execution state.
- `ViewModels/OperationViewModel.cs` — shared operation state, progress, cancellation, logging, and repair-task execution.
- `Services/IDialogService.cs` / `WpfDialogService.cs` — keeps `MessageBox` calls out of ViewModels.
- `Services/IUiDispatcher.cs` / `WpfUiDispatcher.cs` — marshals output callbacks back to the UI thread.
- `Behaviors/TextBoxAutoScrollBehavior.cs` — view behavior for the bound activity log without code-behind.
- `App.xaml.cs` — composition root that creates services/ViewModels and assigns the window DataContext.

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK to build
- Administrator privileges for system maintenance tasks

## Build

```powershell
dotnet clean
dotnet build -c Release
```

Because the application manifest requests administrator rights, either launch the compiled executable normally and accept UAC or use:

```powershell
Start-Process ".\bin\Release\net8.0-windows10.0.19041.0\WindowsRepairToolkit.exe" -Verb RunAs
```

## Publish a self-contained x64 build

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The publish output will be under a path similar to:

```text
bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\
```

## Safety notes

- Debloat removes only explicitly selected current-user AppX packages from the curated candidate list.
- Startup entries are backed up by the toolkit before they are disabled.
- Profile bloat analysis is read-only.
- Component cleanup does not use DISM `/ResetBase`.
- Drive optimization uses Windows' media-aware `/O` behavior.

## MVVM v3 startup fix

The status `ProgressBar` bindings are explicitly `Mode=OneWay`. `OperationViewModel.ProgressValue` and `IsProgressIndeterminate` intentionally expose private setters; WPF's `RangeBase.Value` binding can otherwise default to TwoWay and fail during window construction with a read-only source-property exception.

## Production installers

Installer build files are included under `Installer/` and `Scripts/`.
See `INSTALLERS.md` for Inno Setup and MSIX build/signing instructions.

## Production release signing

Production release metadata and Authenticode signing are documented in `RELEASE-SIGNING.md`.

Typical certificate-store release build:

```powershell
.\Scripts\build-signed-release.ps1 -Version 1.0.0 -Publisher "JK" -CertificateThumbprint "YOUR_CERTIFICATE_THUMBPRINT"
```

The signed release is emitted under `artifacts\release\<version>` with a SHA-256 checksum and `release-manifest.json`.

## Maximized-window taskbar fix

The custom WPF title bar now handles `WM_GETMINMAXINFO` so a maximized window uses the active monitor's Windows work area instead of extending underneath the taskbar. This is monitor-aware and works with taskbars positioned on any edge.

## Portable edition

Build the no-install, self-contained x64 portable ZIP with:

```powershell
.\Scripts\build-portable.ps1 -Version 1.0.0
```

See `PORTABLE.md` for signing and portable-data details.
