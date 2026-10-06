using WindowsRepairToolkit.Tasks.Startup;

namespace WindowsRepairToolkit.ViewModels;

public sealed class StartupItemViewModel : ObservableObject
{
    private bool _isSelected;

    public StartupItemViewModel(StartupEntry entry)
    {
        Entry = entry;
    }

    public StartupEntry Entry { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string Name => Entry.Name;
    public string Command => Entry.Command;
    public string Source => Entry.Source;
    public string Location => Entry.Location;
}
