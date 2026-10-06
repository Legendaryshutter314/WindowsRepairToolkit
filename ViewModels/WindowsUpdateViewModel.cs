using System.Diagnostics;
using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.Tasks.Updates;

namespace WindowsRepairToolkit.ViewModels;

public sealed class WindowsUpdateViewModel
{
    private readonly OperationViewModel _operation;
    private readonly IDialogService _dialogs;

    public WindowsUpdateViewModel(OperationViewModel operation, IDialogService dialogs)
    {
        _operation = operation;
        _dialogs = dialogs;
        InstallUpdatesCommand = new AsyncRelayCommand(
            () => _operation.RunTaskAsync(new WindowsUpdateTask()),
            () => !_operation.IsBusy,
            _operation,
            nameof(OperationViewModel.IsBusy));
        OpenSettingsCommand = new RelayCommand(OpenSettings);
    }

    public AsyncRelayCommand InstallUpdatesCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }

    private void OpenSettings()
    {
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:windowsupdate") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _dialogs.ShowError(ex.Message, "Windows Update");
        }
    }
}
