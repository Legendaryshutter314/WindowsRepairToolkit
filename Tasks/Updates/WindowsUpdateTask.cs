using System.Diagnostics;
using WindowsRepairToolkit.Execution;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Updates;

public sealed class WindowsUpdateTask : IRepairTask
{
    public string Id => "windows-update";
    public string Name => "Install Windows Updates";
    public string Description => "Uses the Windows Update Agent COM API to search, download, and install applicable software updates.";
    public TaskRisk Risk => TaskRisk.Advanced;
    public bool RequiresAdministrator => true;
    public bool RequiresRestart => true;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var runner = new PowerShellRunner();
        var script = @"
$ErrorActionPreference='Stop'
$session = New-Object -ComObject Microsoft.Update.Session
$session.ClientApplicationID = 'Windows Repair Toolkit'
$searcher = $session.CreateUpdateSearcher()
Write-Output 'Searching for updates...'
$result = $searcher.Search(""IsInstalled=0 and Type='Software' and IsHidden=0"")
$updates = New-Object -ComObject Microsoft.Update.UpdateColl
foreach ($u in $result.Updates) {
  Write-Output (""Found: "" + $u.Title)
  if (-not $u.EulaAccepted) { $u.AcceptEula() }
  [void]$updates.Add($u)
}
if ($updates.Count -eq 0) { Write-Output 'No applicable updates found.'; exit 0 }
$downloader = $session.CreateUpdateDownloader(); $downloader.Updates = $updates
Write-Output (""Downloading "" + $updates.Count + "" update(s)..."")
[void]$downloader.Download()
$ready = New-Object -ComObject Microsoft.Update.UpdateColl
foreach ($u in $updates) { if ($u.IsDownloaded) { [void]$ready.Add($u) } }
if ($ready.Count -eq 0) { throw 'No updates were downloaded successfully.' }
$installer = $session.CreateUpdateInstaller(); $installer.Updates = $ready
Write-Output (""Installing "" + $ready.Count + "" update(s)..."")
$install = $installer.Install()
Write-Output (""Installation result code: "" + $install.ResultCode)
Write-Output (""Reboot required: "" + $install.RebootRequired)
if ($install.ResultCode -eq 4) { exit 1 }
";
        var result = await runner.RunAsync(script, context.WriteLog, cancellationToken);
        sw.Stop();
        return new RepairTaskResult
        {
            Success = result.Success,
            ExitCode = result.ExitCode,
            Output = result.Output,
            Error = result.Error,
            RestartRequired = result.Output.Contains("Reboot required: True", StringComparison.OrdinalIgnoreCase),
            Duration = sw.Elapsed
        };
    }
}
