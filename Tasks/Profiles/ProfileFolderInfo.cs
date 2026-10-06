namespace WindowsRepairToolkit.Tasks.Profiles;

public sealed class ProfileFolderInfo
{
    public required string Area { get; init; }
    public required string Path { get; init; }
    public long SizeBytes { get; init; }
    public string SizeDisplay => FormatBytes(SizeBytes);

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var i = 0;
        while (value >= 1024 && i < units.Length - 1) { value /= 1024; i++; }
        return $"{value:0.##} {units[i]}";
    }
}
