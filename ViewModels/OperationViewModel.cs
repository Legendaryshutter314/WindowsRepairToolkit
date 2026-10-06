using System.Text;
using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Models;
using WindowsRepairToolkit.Services;
using WindowsRepairToolkit.Tasks;

namespace WindowsRepairToolkit.ViewModels;

public sealed class OperationViewModel : ObservableObject
{
    private readonly LogService _logService;
    private readonly IDialogService _dialogService;
    private readonly IUiDispatcher _dispatcher;
    private readonly StringBuilder _activityLog = new();
    private CancellationTokenSource? _cts;
    private bool _isBusy;
    private string _statusText = "Ready";
    private double _progressValue;
    private bool _isProgressIndeterminate;
    private string _activityLogText = string.Empty;

    public OperationViewModel(LogService logService, IDialogService dialogService, IUiDispatcher dispatcher)
    {
        _logService = logService;
        _dialogService = dialogService;
        _dispatcher = dispatcher;
        CancelCommand = new RelayCommand(Cancel, () => IsBusy, this, nameof(IsBusy));
    }

    public event EventHandler? OperationCompleted;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set => SetProperty(ref _progressValue, value);
    }

    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        private set => SetProperty(ref _isProgressIndeterminate, value);
    }

    public string ActivityLogText
    {
        get => _activityLogText;
        private set => SetProperty(ref _activityLogText, value);
    }

    public string LogPath => _logService.CurrentLogPath;

    public RelayCommand CancelCommand { get; }

    public async Task<RepairTaskResult?> RunTaskAsync(IRepairTask task, bool skipConfirmation = false)
    {
        if (IsBusy)
        {
            _dialogService.ShowInfo("Another operation is already running.", "Busy");
            return null;
        }

        if (!skipConfirmation && task.Risk != TaskRisk.Safe)
        {
            var warning = task.Risk == TaskRisk.Advanced
                ? "This operation changes Windows configuration or installed content."
                : "This operation can remove user-accessible data or settings.";

            if (!_dialogService.Confirm(
                    $"{task.Name}\n\n{task.Description}\n\n{warning}\n\nContinue?",
                    "Confirm Operation",
                    warning: true))
            {
                return null;
            }
        }

        return await ExecuteTaskCoreAsync(task);
    }

    public async Task<bool> RunExclusiveAsync(
        string status,
        Func<CancellationToken, Task> operation,
        string errorTitle)
    {
        if (IsBusy)
        {
            _dialogService.ShowInfo("Another operation is already running.", "Busy");
            return false;
        }

        Begin(status, indeterminate: true);
        try
        {
            await operation(_cts!.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            AppendLog("Operation canceled by user.");
            StatusText = "Canceled.";
            return false;
        }
        catch (Exception ex)
        {
            AppendLog("ERROR: " + ex);
            StatusText = "Operation failed.";
            _dialogService.ShowError(ex.Message, errorTitle);
            return false;
        }
        finally
        {
            End();
        }
    }

    public void AppendLog(string message)
    {
        _dispatcher.Invoke(() =>
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _activityLog.AppendLine(line);
            ActivityLogText = _activityLog.ToString();
            _logService.Write(message);
        });
    }

    public void SetStatus(string status) => _dispatcher.Invoke(() => StatusText = status);

    private async Task<RepairTaskResult?> ExecuteTaskCoreAsync(IRepairTask task)
    {
        Begin($"Running {task.Name}...", indeterminate: false);
        AppendLog($"=== {task.Name} ===");

        try
        {
            var result = await task.ExecuteAsync(CreateContext(), _cts!.Token);
            if (!string.IsNullOrWhiteSpace(result.Output))
                AppendLog(result.Output.Trim());
            if (!string.IsNullOrWhiteSpace(result.Error))
                AppendLog("ERROR: " + result.Error.Trim());

            AppendLog($"Result: {(result.Success ? "Success" : "Failed")} | Exit code: {result.ExitCode} | Duration: {result.Duration:g}");
            StatusText = result.Success ? $"{task.Name} completed." : $"{task.Name} failed.";
            ProgressValue = 100;

            if (result.RestartRequired)
                AppendLog("A Windows restart may be required.");

            return result;
        }
        catch (OperationCanceledException)
        {
            AppendLog($"{task.Name} canceled by user.");
            StatusText = "Canceled.";
            return null;
        }
        catch (Exception ex)
        {
            AppendLog("ERROR: " + ex);
            StatusText = "Operation failed.";
            _dialogService.ShowError(ex.Message, task.Name);
            return RepairTaskResult.Failed(ex.Message);
        }
        finally
        {
            End();
        }
    }

    private RepairTaskContext CreateContext() => new()
    {
        Log = AppendLog,
        Progress = new Progress<RepairProgress>(p =>
        {
            ProgressValue = p.Percentage;
            StatusText = p.Message;
        })
    };

    private void Begin(string status, bool indeterminate)
    {
        IsBusy = true;
        _cts = new CancellationTokenSource();
        ProgressValue = 0;
        IsProgressIndeterminate = indeterminate;
        StatusText = status;
    }

    private void End()
    {
        IsProgressIndeterminate = false;
        _cts?.Dispose();
        _cts = null;
        IsBusy = false;
        OperationCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel() => _cts?.Cancel();
}
