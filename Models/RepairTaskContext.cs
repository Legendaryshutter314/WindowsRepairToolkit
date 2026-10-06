namespace WindowsRepairToolkit.Models;

public sealed class RepairTaskContext
{
    public IProgress<RepairProgress>? Progress { get; init; }
    public Action<string>? Log { get; init; }

    public void WriteLog(string message) => Log?.Invoke(message);

    public void Report(int percentage, string message) =>
        Progress?.Report(new RepairProgress(Math.Clamp(percentage, 0, 100), message));
}
