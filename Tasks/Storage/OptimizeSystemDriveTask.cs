using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Storage;

public sealed class OptimizeSystemDriveTask : CommandRepairTaskBase
{
    private readonly string _volume;

    public OptimizeSystemDriveTask()
    {
        _volume = Path.GetPathRoot(Environment.SystemDirectory)?.TrimEnd('\\') ?? "C:";
    }

    public override string Id => "optimize-drive";
    public override string Name => "Optimize System Drive";
    public override string Description => "Uses defrag /O so Windows chooses the correct optimization for the media type, including retrim where appropriate.";
    public override TaskRisk Risk => TaskRisk.Safe;
    protected override string FileName => "defrag.exe";
    protected override string Arguments => $"{_volume} /O /U /V";
}
