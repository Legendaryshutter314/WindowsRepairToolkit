namespace WindowsRepairToolkit.Services;

/// <summary>
/// Centralizes application-owned writable paths. Portable mode is enabled when
/// a file named "portable.flag" exists beside the executable (or WRT_PORTABLE=1).
/// </summary>
public static class PortablePaths
{
    private const string ProductFolder = "WindowsRepairToolkit";

    public static string ApplicationDirectory => AppContext.BaseDirectory;

    public static bool IsPortable
    {
        get
        {
            var environmentFlag = Environment.GetEnvironmentVariable("WRT_PORTABLE");
            if (string.Equals(environmentFlag, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(environmentFlag, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return File.Exists(Path.Combine(ApplicationDirectory, "portable.flag"));
        }
    }

    public static string PortableDataRoot => Path.Combine(ApplicationDirectory, "Data");

    public static IEnumerable<string> GetLogDirectoryCandidates()
    {
        if (IsPortable)
        {
            // Keep persistent application state with the portable copy.
            yield return Path.Combine(PortableDataRoot, "Logs");

            // Startup diagnostics still need somewhere to write if the portable
            // media is read-only or unexpectedly unavailable.
            yield return Path.Combine(Path.GetTempPath(), ProductFolder, "Logs");
            yield break;
        }

        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            ProductFolder,
            "Logs");

        yield return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ProductFolder,
            "Logs");

        yield return Path.Combine(Path.GetTempPath(), ProductFolder, "Logs");
    }

    public static string GetStartupBackupDirectory()
    {
        if (IsPortable)
        {
            var portableDirectory = Path.Combine(PortableDataRoot, "StartupBackup");
            Directory.CreateDirectory(portableDirectory);
            return portableDirectory;
        }

        var installedDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            ProductFolder,
            "StartupBackup");

        Directory.CreateDirectory(installedDirectory);
        return installedDirectory;
    }
}
