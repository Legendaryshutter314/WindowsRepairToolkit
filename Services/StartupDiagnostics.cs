using System.Text;

namespace WindowsRepairToolkit.Services;

public static class StartupDiagnostics
{
    private static readonly object Gate = new();
    private static string? _logPath;

    public static string LogPath => _logPath ??= ResolveLogPath();

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.AppendAllText(
                    LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
        }
        catch
        {
            // Startup diagnostics must never become another startup failure.
        }
    }

    public static void WriteException(string context, Exception exception)
    {
        Write($"{context}: {exception}");
    }

    private static string ResolveLogPath()
    {
        foreach (var directory in PortablePaths.GetLogDirectoryCandidates())
        {
            try
            {
                Directory.CreateDirectory(directory);
                return Path.Combine(directory, $"startup-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            }
            catch
            {
                // Try the next writable location.
            }
        }

        return Path.Combine(Path.GetTempPath(), $"WindowsRepairToolkit-startup-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }
}
