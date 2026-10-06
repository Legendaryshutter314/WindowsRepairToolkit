using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.SystemRepair;

public sealed class CreateRestorePointTask : IRepairTask
{
    public string Id => "restore-point";
    public string Name => "Create Restore Point";
    public string Description => "Requests a Windows System Restore checkpoint before making higher-risk changes.";
    public TaskRisk Risk => TaskRisk.Safe;
    public bool RequiresAdministrator => true;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var runner = new PowerShellRunner();
        var description = $"Windows Repair Toolkit {DateTime.Now:yyyy-MM-dd HH:mm}";
        var script = $"Checkpoint-Computer -Description '{description.Replace("'", "''")}' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop; Write-Output 'Restore point requested successfully.'";

        context.Report(10, "Creating restore point...");
        var result = await runner.RunAsync(script, context.WriteLog, cancellationToken);
        sw.Stop();
        context.Report(100, result.Success ? "Restore point completed." : "Restore point could not be created.");

        return new RepairTaskResult
        {
            Success = result.Success,
            ExitCode = result.ExitCode,
            Output = result.Output,
            Error = result.Error,
            Duration = sw.Elapsed
        };
    }
}
