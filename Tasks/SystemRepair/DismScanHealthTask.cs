using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.SystemRepair;

public sealed class DismScanHealthTask : CommandRepairTaskBase
{
    public override string Id => "dism-scanhealth";
    public override string Name => "DISM ScanHealth";
    public override string Description => "Performs a deeper scan of the Windows component store for corruption.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "dism.exe";
    protected override string Arguments => "/Online /Cleanup-Image /ScanHealth";
}
