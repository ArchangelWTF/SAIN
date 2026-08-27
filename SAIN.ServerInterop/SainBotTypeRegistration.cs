using System.Collections.Generic;

namespace SAIN.ServerInterop;

public sealed class SainBotTypeRegistration
{
    /// <summary>Name shown in the SAIN preset editor.</summary>
    public required string Name { get; init; }

    /// <summary>Raw EFT WildSpawnType value the bot spawns as.</summary>
    public required int WildSpawnType { get; init; }

    /// <summary>database/bots/types key to copy EFT difficulty defaults from. Null falls back to the WildSpawnType name.</summary>
    public string? BotDbKey { get; init; }

    public float DifficultyModifier { get; init; } = 0.5f;

    public string Section { get; init; } = "Modded";

    public string? Description { get; init; }

    /// <summary>EFT brain ShortNames SAIN attaches its layers to on the client. If null/empty, SAIN uses BaseBrain.</summary>
    public List<string>? BrainsToApply { get; init; }

    /// <summary>Vanilla layers to strip from those brains so they don't fight SAIN (SAIN adds its common set already).</summary>
    public List<string>? LayersToRemove { get; init; }

    /// <summary>Fallback brain used when BrainsToApply is null/empty.</summary>
    public string? BaseBrain { get; init; }
}
