namespace WindowsRepairToolkit.Execution;

public sealed record CommandResult(int ExitCode, string Output, string Error)
{
    public bool Success => ExitCode == 0;
}
