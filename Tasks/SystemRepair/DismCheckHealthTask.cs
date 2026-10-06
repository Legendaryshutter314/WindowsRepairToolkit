using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.SystemRepair;

public sealed class DismCheckHealthTask : CommandRepairTaskBase
{
    public override string Id => "dism-checkhealth";
    public override string Name => "DISM CheckHealth";
    public override string Description => "Checks whether the Windows component store has been flagged as corrupted.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "dism.exe";
    protected override string Arguments => "/Online /Cleanup-Image /CheckHealth";
}
