using SAIN.Preset.Shared;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Web;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SAINServerMod;

public sealed record SAINServermodMetadata : IModMetadata, IModBlazorMetadata
{
    public string ModGuid { get; init; } = "me.sol.sain";
    public string Name { get; init; } = "SAIN";
    public string Author { get; init; } = "Solarint";
    public List<string>? Contributors { get; init; } = [];
    public Version Version { get; init; } = new(SAINVersionInfo.SAINFullVersion);
    public Range SptVersion { get; init; } = new($"~{SAINVersionInfo.SptVersion}");
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/ArchangelWTF/SAIN";
    public bool? IsBundleMod { get; init; } = false;
    public string License { get; init; } = "MIT";
    public bool HasPrepatcher { get; init; } = false;
    public string? WWWRootUrl { get; init; } = "SAIN";
    public string? HomePage { get; init; } = "/sain/presets";
    public string? HomePageDescription { get; init; } = "Edit various SAIN settings from your browser.";
}
