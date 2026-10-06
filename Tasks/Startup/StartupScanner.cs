using Microsoft.Win32;

namespace WindowsRepairToolkit.Tasks.Startup;

public sealed class StartupScanner
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public Task<List<StartupEntry>> ScanAsync(CancellationToken cancellationToken = default) =>
        Task.Run(() => Scan(cancellationToken), cancellationToken);

    private static List<StartupEntry> Scan(CancellationToken token)
    {
        var items = new List<StartupEntry>();

        ScanRegistry(items, RegistryHive.CurrentUser, RegistryView.Registry64, "HKCU", token);
        ScanRegistry(items, RegistryHive.LocalMachine, RegistryView.Registry64, "HKLM", token);
        ScanRegistry(items, RegistryHive.LocalMachine, RegistryView.Registry32, "HKLM", token);

        AddStartupFolder(items,
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            "Current user Startup folder",
            token);
        AddStartupFolder(items,
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup),
            "All users Startup folder",
            token);

        return items
            .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.Source)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void ScanRegistry(
        ICollection<StartupEntry> items,
        RegistryHive hive,
        RegistryView view,
        string hiveName,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(RunKey, writable: false);
            if (key is null) return;

            foreach (var valueName in key.GetValueNames())
            {
                token.ThrowIfCancellationRequested();
                var data = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames)?.ToString() ?? string.Empty;
                items.Add(new StartupEntry
                {
                    Id = $"registry:{hiveName}:{view}:{RunKey}:{valueName}",
                    Name = valueName,
                    Command = data,
                    Source = "Registry",
                    Location = $"{hiveName} ({view})\\{RunKey}",
                    Hive = hiveName,
                    RegistryViewName = view.ToString(),
                    RegistryPath = RunKey
                });
            }
        }
        catch { }
    }

    private static void AddStartupFolder(
        ICollection<StartupEntry> items,
        string folder,
        string label,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                token.ThrowIfCancellationRequested();
                items.Add(new StartupEntry
                {
                    Id = $"file:{file}",
                    Name = Path.GetFileNameWithoutExtension(file),
                    Command = file,
                    Source = "Startup folder",
                    Location = label,
                    FilePath = file
                });
            }
        }
        catch { }
    }
}
