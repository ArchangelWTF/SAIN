using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace SAINServerMod;

// SPT 4.1 replaced the AbstractModMetadata base record with the IModMetadata
// interface, and swapped IsBundleMod for HasPrepatcher.
public sealed record SAINServermodMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "me.sol.sain";
    public string Name { get; init; } = "SAIN";
    public string Author { get; init; } = "Solarint";
    public List<string>? Contributors { get; init; } = [];
    public Version Version { get; init; } = new("4.5.0");
    public Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; } = [];
    public Dictionary<string, Range>? ModDependencies { get; init; } = [];
    public string? Url { get; init; } = "https://github.com/ArchangelWTF/SAIN";
    public bool HasPrepatcher { get; init; } = false;
    public string License { get; init; } = "MIT";
}
