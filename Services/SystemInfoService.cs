using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace WindowsRepairToolkit.Services;

public sealed record SystemInfoSnapshot(
    string MachineName,
    string UserName,
    string WindowsProductName,
    string WindowsDisplayVersion,
    string OsBuild,
    string Architecture,
    string SystemDrive,
    long SystemDriveFreeBytes,
    long SystemDriveTotalBytes);

public static class SystemInfoService
{
    public static SystemInfoSnapshot GetSnapshot()
    {
        string product = "Windows";
        string displayVersion = "Unknown";
        string build = Environment.OSVersion.Version.Build.ToString();

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            product = key?.GetValue("ProductName")?.ToString() ?? product;
            displayVersion = key?.GetValue("DisplayVersion")?.ToString()
                             ?? key?.GetValue("ReleaseId")?.ToString()
                             ?? displayVersion;
            build = $"{key?.GetValue("CurrentBuildNumber") ?? build}.{key?.GetValue("UBR") ?? 0}";
        }
        catch { }

        var systemRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        var drive = new DriveInfo(systemRoot);

        return new SystemInfoSnapshot(
            Environment.MachineName,
            Environment.UserName,
            product,
            displayVersion,
            build,
            RuntimeInformation.OSArchitecture.ToString(),
            systemRoot,
            drive.IsReady ? drive.AvailableFreeSpace : 0,
            drive.IsReady ? drive.TotalSize : 0);
    }
}
