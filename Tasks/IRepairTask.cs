using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks;

public interface IRepairTask
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    TaskRisk Risk { get; }
    bool RequiresAdministrator { get; }
    bool RequiresRestart { get; }

    Task<RepairTaskResult> ExecuteAsync(
        RepairTaskContext context,
        CancellationToken cancellationToken = default);
}
