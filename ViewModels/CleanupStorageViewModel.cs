using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Tasks.Cleanup;
using WindowsRepairToolkit.Tasks.Storage;

namespace WindowsRepairToolkit.ViewModels;

public sealed class CleanupStorageViewModel
{
    private readonly OperationViewModel _operation;

    public CleanupStorageViewModel(OperationViewModel operation)
    {
        _operation = operation;
        TempCleanupCommand = Create(() => _operation.RunTaskAsync(new TempCleanupTask()));
        RecycleBinCommand = Create(() => _operation.RunTaskAsync(new RecycleBinCleanupTask()));
        ComponentCleanupCommand = Create(() => _operation.RunTaskAsync(new ComponentStoreCleanupTask()));
        DiskCleanupCommand = Create(() => _operation.RunTaskAsync(new DiskCleanupTask()));
        OptimizeDriveCommand = Create(() => _operation.RunTaskAsync(new OptimizeSystemDriveTask()));
    }

    public AsyncRelayCommand TempCleanupCommand { get; }
    public AsyncRelayCommand RecycleBinCommand { get; }
    public AsyncRelayCommand ComponentCleanupCommand { get; }
    public AsyncRelayCommand DiskCleanupCommand { get; }
    public AsyncRelayCommand OptimizeDriveCommand { get; }

    private AsyncRelayCommand Create(Func<Task> action)
        => new(action, () => !_operation.IsBusy, _operation, nameof(OperationViewModel.IsBusy));
}
