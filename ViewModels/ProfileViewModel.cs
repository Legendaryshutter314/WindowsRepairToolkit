using System.Collections.ObjectModel;
using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Tasks.Profiles;

namespace WindowsRepairToolkit.ViewModels;

public sealed class ProfileViewModel
{
    private readonly OperationViewModel _operation;

    public ProfileViewModel(OperationViewModel operation)
    {
        _operation = operation;
        AnalyzeCommand = new AsyncRelayCommand(
            AnalyzeAsync,
            () => !_operation.IsBusy,
            _operation,
            nameof(OperationViewModel.IsBusy));
    }

    public ObservableCollection<ProfileFolderInfo> Items { get; } = [];
    public AsyncRelayCommand AnalyzeCommand { get; }

    private Task AnalyzeAsync()
        => _operation.RunExclusiveAsync(
            "Analyzing profile folder sizes...",
            async token =>
            {
                var scanner = new ProfileScanner();
                var progress = new Progress<string>(_operation.SetStatus);
                var items = await scanner.ScanAsync(progress, token);
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);

                _operation.AppendLog($"Profile analysis completed. Showing the {items.Count} largest measured folders. No files were deleted.");
                _operation.SetStatus("Profile analysis completed.");
            },
            "Profile Analysis");
}
