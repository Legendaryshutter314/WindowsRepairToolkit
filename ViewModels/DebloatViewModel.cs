using System.Collections.ObjectModel;
using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.Tasks.Debloat;

namespace WindowsRepairToolkit.ViewModels;

public sealed class DebloatViewModel
{
    private readonly OperationViewModel _operation;
    private readonly IDialogService _dialogs;

    public DebloatViewModel(OperationViewModel operation, IDialogService dialogs)
    {
        _operation = operation;
        _dialogs = dialogs;
        ScanCommand = Create(ScanAsync);
        RemoveSelectedCommand = Create(RemoveSelectedAsync);
    }

    public ObservableCollection<DebloatCandidateViewModel> Items { get; } = [];

    public AsyncRelayCommand ScanCommand { get; }
    public AsyncRelayCommand RemoveSelectedCommand { get; }

    private AsyncRelayCommand Create(Func<Task> action)
        => new(action, () => !_operation.IsBusy, _operation, nameof(OperationViewModel.IsBusy));

    private Task ScanAsync()
        => _operation.RunExclusiveAsync(
            "Scanning AppX packages...",
            async token =>
            {
                var scanner = new AppxScanner();
                var packages = await scanner.ScanCandidatesAsync(token);
                ReplaceItems(packages);
                _operation.AppendLog($"Found {packages.Count} conservative bloatware candidate(s). No packages were removed.");
                _operation.SetStatus($"Found {packages.Count} candidate(s).");
            },
            "Debloat Scan");

    private async Task RemoveSelectedAsync()
    {
        var selected = Items.Where(x => x.IsSelected).Select(x => x.Package).ToList();
        if (selected.Count == 0)
        {
            _dialogs.ShowInfo("Select one or more packages first.", "Debloat");
            return;
        }

        var names = string.Join(Environment.NewLine, selected.Select(x => "• " + x.Name));
        if (!_dialogs.Confirm(
                $"Remove these {selected.Count} AppX package(s) for the current user?\n\n{names}\n\nThis does not remove provisioned packages for new users.",
                "Confirm App Removal",
                warning: true))
        {
            return;
        }

        foreach (var package in selected)
        {
            var result = await _operation.RunTaskAsync(new AppxRemovalTask(package), skipConfirmation: true);
            if (result is null)
                break;
        }

        await ScanAsync();
    }

    private void ReplaceItems(IEnumerable<AppxPackageInfo> packages)
    {
        Items.Clear();
        foreach (var package in packages)
            Items.Add(new DebloatCandidateViewModel(package));
    }
}
