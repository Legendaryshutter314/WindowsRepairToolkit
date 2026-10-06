using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Tasks.Search;

namespace WindowsRepairToolkit.ViewModels;

public sealed class SearchExplorerViewModel
{
    private readonly OperationViewModel _operation;

    public SearchExplorerViewModel(OperationViewModel operation)
    {
        _operation = operation;
        RebuildIndexCommand = Create(() => _operation.RunTaskAsync(new SearchIndexRebuildTask()));
        RebuildExplorerCommand = Create(() => _operation.RunTaskAsync(new ExplorerCacheRebuildTask()));
    }

    public AsyncRelayCommand RebuildIndexCommand { get; }
    public AsyncRelayCommand RebuildExplorerCommand { get; }

    private AsyncRelayCommand Create(Func<Task> action)
        => new(action, () => !_operation.IsBusy, _operation, nameof(OperationViewModel.IsBusy));
}
