using System.Text;

namespace WindowsRepairToolkit.Execution;

public sealed class PowerShellRunner
{
    private readonly ProcessRunner _processRunner = new();

    public Task<CommandResult> RunAsync(
        string script,
        Action<string>? outputCallback = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.Unicode.GetBytes(script);
        var encoded = Convert.ToBase64String(bytes);
        return _processRunner.RunAsync(
            "powershell.exe",
            $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
            outputCallback,
            cancellationToken);
    }
}
