using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.SystemRepair;

public sealed class SfcScanTask : CommandRepairTaskBase
{
    public override string Id => "sfc-scannow";
    public override string Name => "System File Checker";
    public override string Description => "Scans protected Windows system files and repairs incorrect versions when possible.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "sfc.exe";
    protected override string Arguments => "/scannow";
}
