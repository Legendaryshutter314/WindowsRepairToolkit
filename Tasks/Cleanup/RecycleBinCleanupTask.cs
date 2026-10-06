using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Cleanup;

public sealed class RecycleBinCleanupTask : IRepairTask
{
    public string Id => "recycle-bin";
    public string Name => "Empty Recycle Bin";
    public string Description => "Empties the Recycle Bin for the current user.";
    public TaskRisk Risk => TaskRisk.Moderate;
    public bool RequiresAdministrator => false;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var runner = new PowerShellRunner();
        const string script = "Clear-RecycleBin -Force -ErrorAction Stop; Write-Output 'Recycle Bin emptied.'";
        var result = await runner.RunAsync(script, context.WriteLog, cancellationToken);
        sw.Stop();
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
