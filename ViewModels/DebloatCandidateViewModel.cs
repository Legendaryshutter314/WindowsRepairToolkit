using WindowsRepairToolkit.Tasks.Debloat;

namespace WindowsRepairToolkit.ViewModels;

public sealed class DebloatCandidateViewModel : ObservableObject
{
    private bool _isSelected;

    public DebloatCandidateViewModel(AppxPackageInfo package)
    {
        Package = package;
    }

    public AppxPackageInfo Package { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string Name => Package.Name;
    public string Reason => Package.Reason;
    public string PackageFullName => Package.PackageFullName;
}
