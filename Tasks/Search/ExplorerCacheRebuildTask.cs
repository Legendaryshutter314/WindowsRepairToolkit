using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Search;

public sealed class ExplorerCacheRebuildTask : IRepairTask
{
    public string Id => "explorer-cache-rebuild";
    public string Name => "Rebuild Explorer Icon/Thumbnail Caches";
    public string Description => "Restarts Explorer and removes icon/thumbnail cache database files so Windows recreates them.";
    public TaskRisk Risk => TaskRisk.Advanced;
    public bool RequiresAdministrator => false;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var runner = new PowerShellRunner();
        var script = @"
$ErrorActionPreference='SilentlyContinue'
Stop-Process -Name explorer -Force
Start-Sleep -Milliseconds 800
$explorerCache = Join-Path $env:LOCALAPPDATA 'Microsoft\Windows\Explorer'
Remove-Item (Join-Path $env:LOCALAPPDATA 'IconCache.db') -Force
Remove-Item (Join-Path $explorerCache 'iconcache*.db') -Force
Remove-Item (Join-Path $explorerCache 'thumbcache*.db') -Force
Start-Process explorer.exe
Write-Output 'Explorer icon and thumbnail caches cleared and Explorer restarted.'
";
        var result = await runner.RunAsync(script, context.WriteLog, cancellationToken);
        sw.Stop();
        return new RepairTaskResult { Success = result.Success, ExitCode = result.ExitCode, Output = result.Output, Error = result.Error, Duration = sw.Elapsed };
    }
}
