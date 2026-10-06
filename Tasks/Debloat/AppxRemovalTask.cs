using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Debloat;

public sealed class AppxRemovalTask(AppxPackageInfo package) : IRepairTask
{
    public string Id => "appx-remove";
    public string Name => $"Remove {package.Name}";
    public string Description => "Removes the selected AppX package for the current user only. Provisioned packages and other users are not modified.";
    public TaskRisk Risk => TaskRisk.Advanced;
    public bool RequiresAdministrator => false;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var runner = new PowerShellRunner();
        var full = package.PackageFullName.Replace("'", "''");
        var script = $"Remove-AppxPackage -Package '{full}' -ErrorAction Stop; Write-Output 'Removed {package.Name.Replace("'", "''")}'";
        var result = await runner.RunAsync(script, context.WriteLog, cancellationToken);
        sw.Stop();
        return new RepairTaskResult { Success = result.Success, ExitCode = result.ExitCode, Output = result.Output, Error = result.Error, Duration = sw.Elapsed };
    }
}
