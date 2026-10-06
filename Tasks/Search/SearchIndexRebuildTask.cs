using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsRepairToolkit.Models;

namespace WindowsRepairToolkit.Tasks.Search;

public sealed class SearchIndexRebuildTask : IRepairTask
{
    public string Id => "search-index-reset";
    public string Name => "Rebuild Windows Search Index";
    public string Description => "Calls the Windows Search SystemIndex catalog Reset method. Search results may be incomplete while the index is rebuilt.";
    public TaskRisk Risk => TaskRisk.Advanced;
    public bool RequiresAdministrator => true;
    public bool RequiresRestart => false;

    public async Task<RepairTaskResult> ExecuteAsync(RepairTaskContext context, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                context.Report(10, "Connecting to Windows Search...");
                var clsid = new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39");
                var type = Type.GetTypeFromCLSID(clsid, throwOnError: true)!;
                dynamic manager = Activator.CreateInstance(type)!;
                try
                {
                    dynamic catalog = manager.GetCatalog("SystemIndex");
                    try { catalog.Reset(); }
                    finally { if (Marshal.IsComObject(catalog)) Marshal.FinalReleaseComObject(catalog); }
                }
                finally { if (Marshal.IsComObject(manager)) Marshal.FinalReleaseComObject(manager); }
            }, cancellationToken);
            sw.Stop();
            context.Report(100, "Windows Search index rebuild was requested.");
            return new RepairTaskResult { Success = true, ExitCode = 0, Output = "SystemIndex reset requested.", Duration = sw.Elapsed };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            return RepairTaskResult.Failed($"Windows Search reset failed: {ex.Message}");
        }
    }
}
