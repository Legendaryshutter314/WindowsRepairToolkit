using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.SystemRepair;

public sealed class DismRestoreHealthTask : CommandRepairTaskBase
{
    public override string Id => "dism-restorehealth";
    public override string Name => "DISM RestoreHealth";
    public override string Description => "Scans and repairs the Windows component store, using Windows Update as a source when needed.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "dism.exe";
    protected override string Arguments => "/Online /Cleanup-Image /RestoreHealth";
}
