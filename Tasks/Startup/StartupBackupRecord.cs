namespace WindowsRepairToolkit.Tasks.Startup;

public sealed class StartupBackupRecord
{
    public Guid BackupId { get; init; } = Guid.NewGuid();
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public required string EntryName { get; init; }
    public required string Source { get; init; }
    public string? Hive { get; init; }
    public string? RegistryViewName { get; init; }
    public string? RegistryPath { get; init; }
    public string? RegistryValueName { get; init; }
    public string? RegistryValueData { get; init; }
    public string? RegistryValueKindName { get; init; }
    public string? OriginalFilePath { get; init; }
    public string? BackupFilePath { get; init; }
    public bool Restored { get; set; }
}
