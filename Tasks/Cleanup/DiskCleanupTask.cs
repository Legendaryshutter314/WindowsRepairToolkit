using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Cleanup;

public sealed class DiskCleanupTask : CommandRepairTaskBase
{
    public override string Id => "disk-cleanup";
    public override string Name => "Legacy Disk Cleanup";
    public override string Description => "Runs the built-in Disk Cleanup utility in very-low-disk mode when cleanmgr.exe is available.";
    public override TaskRisk Risk => TaskRisk.Moderate;
    protected override string FileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cleanmgr.exe");
    protected override string Arguments => "/VERYLOWDISK";

    public override async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(FileName))
            return RepairTaskResult.Failed("cleanmgr.exe is not available on this Windows installation.");
        return await base.ExecuteAsync(context, cancellationToken);
    }
}
