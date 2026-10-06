using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.Tasks.SystemRepair;

namespace WindowsRepairToolkit.ViewModels;

public sealed class SystemRepairViewModel
{
    private readonly OperationViewModel _operation;
    private readonly IDialogService _dialogs;

    public SystemRepairViewModel(OperationViewModel operation, IDialogService dialogs)
    {
        _operation = operation;
        _dialogs = dialogs;

        RecommendedRepairCommand = Create(RecommendedRepairAsync);
        CreateRestorePointCommand = Create(() => _operation.RunTaskAsync(new CreateRestorePointTask()));
        DismCheckCommand = Create(() => _operation.RunTaskAsync(new DismCheckHealthTask()));
        DismScanCommand = Create(() => _operation.RunTaskAsync(new DismScanHealthTask()));
        DismRestoreCommand = Create(() => _operation.RunTaskAsync(new DismRestoreHealthTask()));
        SfcCommand = Create(() => _operation.RunTaskAsync(new SfcScanTask()));
    }

    public AsyncRelayCommand RecommendedRepairCommand { get; }
    public AsyncRelayCommand CreateRestorePointCommand { get; }
    public AsyncRelayCommand DismCheckCommand { get; }
    public AsyncRelayCommand DismScanCommand { get; }
    public AsyncRelayCommand DismRestoreCommand { get; }
    public AsyncRelayCommand SfcCommand { get; }

    private AsyncRelayCommand Create(Func<Task> action)
        => new(action, () => !_operation.IsBusy, _operation, nameof(OperationViewModel.IsBusy));

    private async Task RecommendedRepairAsync()
    {
        if (!_dialogs.Confirm(
                "Run the recommended system repair sequence?\n\n1. DISM RestoreHealth\n2. SFC /scannow\n\nThis can take a while and requires an Internet connection if DISM needs Windows Update as a repair source.",
                "Recommended Repair"))
        {
            return;
        }

        var dism = await _operation.RunTaskAsync(new DismRestoreHealthTask(), skipConfirmation: true);
        if (dism?.Success == true)
            await _operation.RunTaskAsync(new SfcScanTask(), skipConfirmation: true);
        else
            _operation.AppendLog("SFC was not started automatically because DISM did not complete successfully.");
    }
}
