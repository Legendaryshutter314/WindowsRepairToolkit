namespace WindowsRepairToolkit.Tasks.Startup;

public sealed class StartupEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Command { get; init; }
    public required string Source { get; init; }
    public required string Location { get; init; }
    public string? Hive { get; init; }
    public string? RegistryViewName { get; init; }
    public string? RegistryPath { get; init; }
    public string? FilePath { get; init; }
}
