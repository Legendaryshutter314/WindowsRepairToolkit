namespace WindowsRepairToolkit.Tasks.Debloat;

public sealed class AppxPackageInfo
{
    public required string Name { get; init; }
    public required string PackageFullName { get; init; }
    public bool NonRemovable { get; init; }
    public bool IsFramework { get; init; }
    public required string Reason { get; init; }
}
