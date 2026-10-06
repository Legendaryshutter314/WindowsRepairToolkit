using System.Text;

namespace WindowsRepairToolkit.Services;

public sealed class LogService
{
    private readonly object _gate = new();

    public string CurrentLogPath { get; }

    public LogService()
    {
        CurrentLogPath = CreateWritableLogPath();
    }

    public void Write(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        lock (_gate)
        {
            File.AppendAllText(CurrentLogPath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    private static string CreateWritableLogPath()
    {
        foreach (var directory in PortablePaths.GetLogDirectoryCandidates())
        {
            try
            {
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, $"WRT-{DateTime.Now:yyyyMMdd-HHmmss}.log");

                // Verify the selected directory is writable now rather than
                // allowing the first repair operation to fail later.
                using (File.Create(path)) { }
                return path;
            }
            catch
            {
                // Try the next candidate.
            }
        }

        throw new IOException("Unable to create a writable Windows Repair Toolkit log directory.");
    }
}
