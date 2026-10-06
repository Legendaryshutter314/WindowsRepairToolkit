using System.Text.Json;
using WindowsRepairToolkit.Execution;

namespace WindowsRepairToolkit.Tasks.Debloat;

public sealed class AppxScanner
{
    private static readonly (string Pattern, string Reason)[] CandidatePatterns =
    [
        ("Microsoft.Xbox", "Xbox consumer app"),
        ("Microsoft.GamingApp", "Xbox/Gaming app"),
        ("Microsoft.BingNews", "News app"),
        ("Microsoft.BingWeather", "Weather app"),
        ("Microsoft.GetHelp", "Get Help app"),
        ("Microsoft.Getstarted", "Tips/Get Started app"),
        ("Microsoft.MicrosoftSolitaireCollection", "Solitaire"),
        ("Microsoft.People", "People app"),
        ("Microsoft.ZuneMusic", "Media consumer app"),
        ("Microsoft.ZuneVideo", "Media consumer app"),
        ("Microsoft.MixedReality", "Mixed Reality app"),
        ("Clipchamp.Clipchamp", "Clipchamp"),
        ("MicrosoftTeams", "Consumer Teams package")
    ];

    public async Task<List<AppxPackageInfo>> ScanCandidatesAsync(CancellationToken token = default)
    {
        var runner = new PowerShellRunner();
        const string script = "Get-AppxPackage | Select-Object Name,PackageFullName,NonRemovable,IsFramework | ConvertTo-Json -Compress";
        var result = await runner.RunAsync(script, cancellationToken: token);
        if (!result.Success) throw new InvalidOperationException(result.Error);
        if (string.IsNullOrWhiteSpace(result.Output)) return [];

        using var doc = JsonDocument.Parse(result.Output);
        var elements = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToArray()
            : [doc.RootElement];

        var packages = new List<AppxPackageInfo>();
        foreach (var e in elements)
        {
            token.ThrowIfCancellationRequested();
            var name = GetString(e, "Name");
            var full = GetString(e, "PackageFullName");
            var nonRemovable = GetBool(e, "NonRemovable");
            var framework = GetBool(e, "IsFramework");
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(full) || nonRemovable || framework) continue;

            var match = CandidatePatterns.FirstOrDefault(p => name.StartsWith(p.Pattern, StringComparison.OrdinalIgnoreCase));
            if (match.Pattern is null) continue;

            packages.Add(new AppxPackageInfo
            {
                Name = name,
                PackageFullName = full,
                NonRemovable = nonRemovable,
                IsFramework = framework,
                Reason = match.Reason
            });
        }
        return packages.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string GetString(JsonElement e, string name) =>
        e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() ?? string.Empty : string.Empty;

    private static bool GetBool(JsonElement e, string name) =>
        e.TryGetProperty(name, out var p) && (p.ValueKind == JsonValueKind.True || p.ValueKind == JsonValueKind.False) && p.GetBoolean();
}
