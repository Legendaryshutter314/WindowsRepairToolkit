using System.Text.Json;
using WindowsRepairToolkit.Services;

namespace WindowsRepairToolkit.Tasks.Startup;

public sealed class StartupBackupStore
{
    private readonly string _root;
    private readonly string _jsonPath;

    public StartupBackupStore()
    {
        _root = PortablePaths.GetStartupBackupDirectory();
        _jsonPath = Path.Combine(_root, "startup-backups.json");
    }

    public string BackupDirectory => _root;

    public async Task<List<StartupBackupRecord>> LoadAsync()
    {
        if (!File.Exists(_jsonPath)) return [];
        await using var stream = File.OpenRead(_jsonPath);
        return await JsonSerializer.DeserializeAsync<List<StartupBackupRecord>>(stream)
               ?? [];
    }

    public async Task SaveAsync(List<StartupBackupRecord> records)
    {
        var temp = _jsonPath + ".tmp";
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, records, new JsonSerializerOptions { WriteIndented = true });
        }
        File.Move(temp, _jsonPath, overwrite: true);
    }
}
