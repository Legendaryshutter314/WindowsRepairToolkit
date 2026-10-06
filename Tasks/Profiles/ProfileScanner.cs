namespace WindowsRepairToolkit.Tasks.Profiles;

public sealed class ProfileScanner
{
    public Task<List<ProfileFolderInfo>> ScanAsync(IProgress<string>? progress = null, CancellationToken token = default) =>
        Task.Run(() => Scan(progress, token), token);

    private static List<ProfileFolderInfo> Scan(IProgress<string>? progress, CancellationToken token)
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var roots = new[]
        {
            ("Profile", profile),
            ("AppData Local", Path.Combine(profile, "AppData", "Local")),
            ("AppData Roaming", Path.Combine(profile, "AppData", "Roaming"))
        };
        var results = new List<ProfileFolderInfo>();

        foreach (var (area, root) in roots)
        {
            if (!Directory.Exists(root)) continue;
            IEnumerable<string> dirs;
            try { dirs = Directory.EnumerateDirectories(root).ToArray(); } catch { continue; }
            foreach (var dir in dirs)
            {
                token.ThrowIfCancellationRequested();
                progress?.Report($"Measuring {dir}");
                results.Add(new ProfileFolderInfo { Area = area, Path = dir, SizeBytes = Measure(dir, token) });
            }
        }

        return results.OrderByDescending(x => x.SizeBytes).Take(100).ToList();
    }

    private static long Measure(string dir, CancellationToken token)
    {
        long total = 0;
        try
        {
            var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
            foreach (var file in Directory.EnumerateFiles(dir, "*", options))
            {
                token.ThrowIfCancellationRequested();
                try { total += new FileInfo(file).Length; } catch { }
            }
        }
        catch { }
        return total;
    }
}
