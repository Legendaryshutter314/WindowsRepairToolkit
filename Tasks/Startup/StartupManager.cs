using Microsoft.Win32;

namespace WindowsRepairToolkit.Tasks.Startup;

public sealed class StartupManager
{
    private readonly StartupBackupStore _store = new();

    public async Task<string> DisableAsync(StartupEntry entry)
    {
        var records = await _store.LoadAsync();

        if (entry.Source == "Registry")
        {
            if (entry.Hive is null || entry.RegistryViewName is null || entry.RegistryPath is null)
                throw new InvalidOperationException("Incomplete registry startup entry metadata.");

            var hive = entry.Hive == "HKLM" ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
            var view = Enum.Parse<RegistryView>(entry.RegistryViewName);
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(entry.RegistryPath, writable: true)
                ?? throw new InvalidOperationException("Startup registry key is no longer present.");

            var current = key.GetValue(entry.Name, null, RegistryValueOptions.DoNotExpandEnvironmentNames)?.ToString()
                ?? throw new InvalidOperationException("Startup value is no longer present.");
            var currentKind = key.GetValueKind(entry.Name);

            records.Add(new StartupBackupRecord
            {
                EntryName = entry.Name,
                Source = "Registry",
                Hive = entry.Hive,
                RegistryViewName = entry.RegistryViewName,
                RegistryPath = entry.RegistryPath,
                RegistryValueName = entry.Name,
                RegistryValueData = current,
                RegistryValueKindName = currentKind.ToString()
            });

            key.DeleteValue(entry.Name, throwOnMissingValue: false);
            await _store.SaveAsync(records);
            return $"Disabled startup registry entry: {entry.Name}";
        }

        if (entry.Source == "Startup folder" && entry.FilePath is not null)
        {
            if (!File.Exists(entry.FilePath))
                throw new FileNotFoundException("Startup file no longer exists.", entry.FilePath);

            var backupName = $"{Guid.NewGuid():N}-{Path.GetFileName(entry.FilePath)}";
            var backupPath = Path.Combine(_store.BackupDirectory, backupName);
            File.Move(entry.FilePath, backupPath);

            records.Add(new StartupBackupRecord
            {
                EntryName = entry.Name,
                Source = "Startup folder",
                OriginalFilePath = entry.FilePath,
                BackupFilePath = backupPath
            });

            await _store.SaveAsync(records);
            return $"Disabled Startup-folder item: {entry.Name}";
        }

        throw new InvalidOperationException("Unsupported startup entry source.");
    }

    public async Task<List<string>> RestoreAllAsync()
    {
        var records = await _store.LoadAsync();
        var messages = new List<string>();

        foreach (var record in records.Where(r => !r.Restored).OrderByDescending(r => r.CreatedUtc))
        {
            try
            {
                if (record.Source == "Registry")
                {
                    var hive = record.Hive == "HKLM" ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
                    var view = Enum.Parse<RegistryView>(record.RegistryViewName!);
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var key = baseKey.CreateSubKey(record.RegistryPath!, writable: true);
                    var kind = Enum.TryParse<RegistryValueKind>(record.RegistryValueKindName, out var parsedKind) ? parsedKind : RegistryValueKind.String;
                    key.SetValue(record.RegistryValueName!, record.RegistryValueData ?? string.Empty, kind);
                }
                else if (record.Source == "Startup folder")
                {
                    if (record.BackupFilePath is null || record.OriginalFilePath is null || !File.Exists(record.BackupFilePath))
                        continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(record.OriginalFilePath)!);
                    File.Move(record.BackupFilePath, record.OriginalFilePath, overwrite: true);
                }

                record.Restored = true;
                messages.Add($"Restored: {record.EntryName}");
            }
            catch (Exception ex)
            {
                messages.Add($"Could not restore {record.EntryName}: {ex.Message}");
            }
        }

        await _store.SaveAsync(records);
        return messages;
    }
}
