using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Cleanup;

public sealed class ComponentStoreCleanupTask : CommandRepairTaskBase
{
    public override string Id => "component-cleanup";
    public override string Name => "Windows Component Cleanup";
    public override string Description => "Runs DISM StartComponentCleanup without ResetBase, preserving the normal ability to uninstall applicable updates.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "dism.exe";
    protected override string Arguments => "/Online /Cleanup-Image /StartComponentCleanup";
}
