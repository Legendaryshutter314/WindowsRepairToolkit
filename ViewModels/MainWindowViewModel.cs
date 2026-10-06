using WindowsRepairToolkit.Commands;
using WindowsRepairToolkit.Services;

namespace WindowsRepairToolkit.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly IDialogService _dialogs;
    private string _systemSummary = "Loading system information...";
    private bool _isActivityLogVisible;

    public MainWindowViewModel(
        OperationViewModel operation,
        IDialogService dialogs,
        SystemRepairViewModel systemRepair,
        CleanupStorageViewModel cleanupStorage,
        StartupViewModel startup,
        DebloatViewModel debloat,
        ProfileViewModel profile,
        SearchExplorerViewModel searchExplorer,
        WindowsUpdateViewModel windowsUpdate)
    {
        Operation = operation;
        _dialogs = dialogs;
        SystemRepair = systemRepair;
        CleanupStorage = cleanupStorage;
        Startup = startup;
        Debloat = debloat;
        Profile = profile;
        SearchExplorer = searchExplorer;
        WindowsUpdate = windowsUpdate;

        ToggleActivityLogCommand = new RelayCommand(() => IsActivityLogVisible = !IsActivityLogVisible);
        Operation.OperationCompleted += (_, _) => LoadSystemSummary();

        LoadSystemSummary();

        if (!AdministratorHelper.IsAdministrator())
        {
            _dialogs.ShowWarning(
                "This application is designed to run elevated. Restart it as Administrator.",
                "Elevation Required");
        }
    }

    public OperationViewModel Operation { get; }
    public SystemRepairViewModel SystemRepair { get; }
    public CleanupStorageViewModel CleanupStorage { get; }
    public StartupViewModel Startup { get; }
    public DebloatViewModel Debloat { get; }
    public ProfileViewModel Profile { get; }
    public SearchExplorerViewModel SearchExplorer { get; }
    public WindowsUpdateViewModel WindowsUpdate { get; }

    public string SystemSummary
    {
        get => _systemSummary;
        private set => SetProperty(ref _systemSummary, value);
    }

    public bool IsActivityLogVisible
    {
        get => _isActivityLogVisible;
        set
        {
            if (SetProperty(ref _isActivityLogVisible, value))
                OnPropertyChanged(nameof(ToggleActivityLogText));
        }
    }

    public string ToggleActivityLogText => IsActivityLogVisible ? "Hide Activity Log" : "Show Activity Log";

    public RelayCommand ToggleActivityLogCommand { get; }

    private void LoadSystemSummary()
    {
        try
        {
            var s = SystemInfoService.GetSnapshot();
            SystemSummary = $"{s.WindowsProductName} {s.WindowsDisplayVersion} | Build {s.OsBuild} | {s.Architecture} | " +
                            $"{s.SystemDrive} free {FormatBytes(s.SystemDriveFreeBytes)} / {FormatBytes(s.SystemDriveTotalBytes)} | " +
                            $"{s.MachineName}\\{s.UserName}";
        }
        catch (Exception ex)
        {
            SystemSummary = $"System information unavailable: {ex.Message}";
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var i = 0;
        while (value >= 1024 && i < units.Length - 1)
        {
            value /= 1024;
            i++;
        }

        return $"{value:0.##} {units[i]}";
    }
}
