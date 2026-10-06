using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks;

public abstract class CommandRepairTaskBase : IRepairTask
{
    protected readonly ProcessRunner Runner = new();

    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract TaskRisk Risk { get; }
    public virtual bool RequiresAdministrator => true;
    public virtual bool RequiresRestart => false;
    protected abstract string FileName { get; }
    protected abstract string Arguments { get; }

    public virtual async Task<RepairTaskResult> ExecuteAsync(
        RepairTaskContext context,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        context.Report(5, $"Starting {Name}...");
        context.WriteLog($"> {FileName} {Arguments}");

        try
        {
            var result = await Runner.RunAsync(
                FileName,
                Arguments,
                line => context.WriteLog(line),
                cancellationToken);

            sw.Stop();
            context.Report(100, result.Success ? $"{Name} completed." : $"{Name} failed.");

            return new RepairTaskResult
            {
                Success = result.Success,
                ExitCode = result.ExitCode,
                Output = result.Output,
                Error = result.Error,
                RestartRequired = RequiresRestart,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            return RepairTaskResult.Failed(ex.Message);
        }
    }
}
