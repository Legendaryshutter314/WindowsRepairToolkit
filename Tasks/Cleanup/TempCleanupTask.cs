using System.Diagnostics;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Cleanup;

public sealed class TempCleanupTask : IRepairTask
{
    public string Id => "temp-cleanup";
    public string Name => "Clean Temporary Files";
    public string Description => "Deletes accessible files from the current-user and Windows temporary folders. In-use files are skipped.";
    public TaskRisk Risk => TaskRisk.Safe;
    public bool RequiresAdministrator => true;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        long bytesFreed = 0;
        int deleted = 0;
        int skipped = 0;
        var paths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        }.Distinct(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(path)) continue;
                context.WriteLog($"Cleaning: {path}");

                IEnumerable<string> entries;
                try { entries = Directory.EnumerateFileSystemEntries(path); }
                catch { continue; }

                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        if (File.Exists(entry))
                        {
                            long length = 0;
                            try { length = new FileInfo(entry).Length; } catch { }
                            File.SetAttributes(entry, FileAttributes.Normal);
                            File.Delete(entry);
                            bytesFreed += length;
                            deleted++;
                        }
                        else if (Directory.Exists(entry))
                        {
                            var size = TryGetDirectorySize(entry, cancellationToken);
                            Directory.Delete(entry, recursive: true);
                            bytesFreed += size;
                            deleted++;
                        }
                    }
                    catch
                    {
                        skipped++;
                    }
                }
            }
        }, cancellationToken);

        sw.Stop();
        var summary = $"Deleted {deleted:N0} entries; skipped {skipped:N0}; approximately {FormatBytes(bytesFreed)} freed.";
        context.WriteLog(summary);
        context.Report(100, summary);

        return new RepairTaskResult
        {
            Success = true,
            ExitCode = 0,
            Output = summary,
            Duration = sw.Elapsed
        };
    }

    private static long TryGetDirectorySize(string path, CancellationToken token)
    {
        long total = 0;
        try
        {
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint
            };
            foreach (var file in Directory.EnumerateFiles(path, "*", options))
            {
                token.ThrowIfCancellationRequested();
                try { total += new FileInfo(file).Length; } catch { }
            }
        }
        catch { }
        return total;
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int index = 0;
        while (value >= 1024 && index < units.Length - 1)
        {
            value /= 1024;
            index++;
        }
        return $"{value:0.##} {units[index]}";
    }
}
