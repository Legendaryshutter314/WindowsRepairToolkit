using System.Collections.ObjectModel;
using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.Tasks.Startup;

namespace WindowsRepairToolkit.ViewModels;

public sealed class StartupViewModel
{
    private readonly OperationViewModel _operation;
    private readonly IDialogService _dialogs;

    public StartupViewModel(OperationViewModel operation, IDialogService dialogs)
    {
        _operation = operation;
        _dialogs = dialogs;

        ScanCommand = Create(ScanAsync);
        DisableSelectedCommand = Create(DisableSelectedAsync);
        RestoreDisabledCommand = Create(RestoreDisabledAsync);
    }

    public ObservableCollection<StartupItemViewModel> Items { get; } = [];

    public AsyncRelayCommand ScanCommand { get; }
    public AsyncRelayCommand DisableSelectedCommand { get; }
    public AsyncRelayCommand RestoreDisabledCommand { get; }

    private AsyncRelayCommand Create(Func<Task> action)
        => new(action, () => !_operation.IsBusy, _operation, nameof(OperationViewModel.IsBusy));

    private Task ScanAsync()
        => _operation.RunExclusiveAsync(
            "Scanning startup items...",
            async token =>
            {
                var scanner = new StartupScanner();
                var items = await scanner.ScanAsync(token);
                ReplaceItems(items);
                _operation.AppendLog($"Found {items.Count} startup item(s). No changes were made.");
                _operation.SetStatus($"Found {items.Count} startup item(s).");
            },
            "Startup Scan");

    private async Task DisableSelectedAsync()
    {
        var selected = Items.Where(x => x.IsSelected).Select(x => x.Entry).ToList();
        if (selected.Count == 0)
        {
            _dialogs.ShowInfo("Select one or more startup entries first.", "Startup");
            return;
        }

        if (!_dialogs.Confirm(
                $"Disable {selected.Count} selected startup item(s)?\n\nEach item is backed up before it is removed from its startup location.",
                "Disable Startup Items",
                warning: true))
        {
            return;
        }

        await _operation.RunExclusiveAsync(
            "Disabling startup items...",
            async token =>
            {
                var manager = new StartupManager();
                foreach (var item in selected)
                {
                    token.ThrowIfCancellationRequested();
                    var message = await manager.DisableAsync(item);
                    _operation.AppendLog(message);
                }

                await RefreshAsync(token);
                _operation.SetStatus("Selected startup items disabled.");
            },
            "Startup");
    }

    private async Task RestoreDisabledAsync()
    {
        if (!_dialogs.Confirm(
                "Restore all startup entries previously disabled by this toolkit?",
                "Restore Startup Items"))
        {
            return;
        }

        await _operation.RunExclusiveAsync(
            "Restoring startup items...",
            async token =>
            {
                var manager = new StartupManager();
                var messages = await manager.RestoreAllAsync();
                foreach (var message in messages)
                    _operation.AppendLog(message);

                await RefreshAsync(token);
                _operation.SetStatus(messages.Count == 0
                    ? "No startup backups required restoration."
                    : "Startup backups restored.");
            },
            "Startup Restore");
    }

    private async Task RefreshAsync(CancellationToken token)
    {
        var scanner = new StartupScanner();
        var items = await scanner.ScanAsync(token);
        ReplaceItems(items);
    }

    private void ReplaceItems(IEnumerable<StartupEntry> items)
    {
        Items.Clear();
        foreach (var item in items)
            Items.Add(new StartupItemViewModel(item));
    }
}
