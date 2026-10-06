namespace WindowsRepairToolkit.Models;

public sealed record RepairTaskResult
{
    public required bool Success { get; init; }
    public int ExitCode { get; init; }
    public string? Output { get; init; }
    public string? Error { get; init; }
    public bool RestartRequired { get; init; }
    public TimeSpan Duration { get; init; }

    public static RepairTaskResult Failed(string error, int exitCode = -1) => new()
    {
        Success = false,
        ExitCode = exitCode,
        Error = error
    };
}
