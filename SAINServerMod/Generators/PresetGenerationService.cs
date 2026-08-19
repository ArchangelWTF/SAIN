using SAIN.Preset.Shared.BotSettings;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Preset;
using SAINServerMod.Extensions;
using SAINServerMod.Models.Preset;
using SAINServerMod.Services;
using SAINServerMod.Utils;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils.Cloners;

namespace SAINServerMod.Generators;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 5)]
public sealed class PresetGenerationService(
    BotTable botTable,
    PresetService presetService,
    ICloner cloner,
    ISptLogger<PresetGenerationService> logger
) : IOnLoad
{
    private static readonly ESainBotDifficulty[] _difficulties =
    [
        ESainBotDifficulty.easy,
        ESainBotDifficulty.normal,
        ESainBotDifficulty.hard,
        ESainBotDifficulty.impossible,
    ];

    private static readonly DefaultPreset[] _defaults =
    [
        new(SAINDifficulty.easy, "Baby Bots", "Bots react slowly and are incredibly inaccurate."),
        new(SAINDifficulty.lesshard, "Less Difficult", "Bots react more slowly, and are less accurate than usual."),
        new(SAINDifficulty.hard, "Default", "Bots are difficult but fair, the way SAIN was meant to played."),
        new(SAINDifficulty.harderpmcs, "Default with Harder PMCs", "Default Settings, but PMCs are harder than normal."),
        new(SAINDifficulty.veryhard, "I Like Pain", "Bots react faster, are more accurate, and can see further."),
        new(
            SAINDifficulty.deathwish,
            "Death Wish",
            "Prepare To Die. Bots have almost no scatter, get less recoil from their weapon while shooting, are more accurate, and react deadly fast."
        ),
    ];

    public List<GeneratedPreset> Presets { get; set; } = [];

    public async Task OnLoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            GenerateDefaults();
            logger.Success($"[SAIN] Generated {Presets.Count} default presets");
        }
        catch (Exception ex)
        {
            logger.Error($"[SAIN] Failed to generate default presets on boot: {ex}");
            throw;
        }

        try
        {
            var imported = await presetService.ImportCustomDirectoriesAsync();
            if (imported.Count > 0)
            {
                logger.Info($"[SAIN] Imported {imported.Count} directory preset(s) as custom: {string.Join(", ", imported)}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[SAIN] Failed to import directory presets: {ex.Message}");
        }

        try
        {
            await presetService.UpgradeCustomToCurrentSchemaAsync();
        }
        catch (Exception ex)
        {
            logger.Error($"[SAIN] Failed to bring custom presets up to date: {ex.Message}");
        }
    }

    public GeneratedPreset Generate(SAINDifficulty difficulty, string name, string description)
    {
        var global = new GlobalSettingsClass();
        var botSettings = GenerateBotSettings(DefaultBotTypes.All(), _difficulties);

        difficulty.ApplyTuning(global, botSettings);

        var personalities = PersonalityGenerator.BuildDefaults();
        var gearStealth = GearStealthGenerator.BuildDefaults();
        var info = SAINPresetDefinition.CreateDefaultDefinition(name, difficulty, description);
        return new GeneratedPreset(info, global, botSettings, personalities, gearStealth);
    }

    private Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> GenerateBotSettings(
        IEnumerable<BotTypeInfo> botTypes,
        ESainBotDifficulty[] difficulties
    )
    {
        var result = new Dictionary<ESainWildSpawnType, SAINSettingsGroupClass>();

        foreach (BotTypeInfo botType in botTypes)
        {
            var group = new SAINSettingsGroupClass(difficulties)
            {
                Name = botType.Name,
                WildSpawnType = botType.WildSpawnType,
                DifficultyModifier = botType.DefaultDifficultyModifier,
            };

            foreach (KeyValuePair<ESainBotDifficulty, SAINSettingsClass> pair in group.Settings)
            {
                var defaults = GetDefaults(botType.WildSpawnType, pair.Key);

                if (defaults != null)
                {
                    SettingsCopier.CopyValues(defaults, pair.Value, SettingsCopier.ShouldCopyDefault);
                }
            }

            result[botType.WildSpawnType] = group;
        }

        return result;
    }

    private DifficultyCategories? GetDefaults(ESainWildSpawnType type, ESainBotDifficulty difficulty)
    {
        var types = botTable.Types;

        if (!types.TryGetValue(type.ToString().ToLowerInvariant(), out var botType) || botType?.BotDifficulty is null)
        {
            return null;
        }

        return botType.BotDifficulty.TryGetValue(difficulty.ToString().ToLowerInvariant(), out var categories)
            ? cloner.Clone(categories)
            : null;
    }

    public List<GeneratedPreset> GenerateDefaults()
    {
        var result = new List<GeneratedPreset>();

        foreach (var (difficulty, name, description) in _defaults)
        {
            result.Add(Generate(difficulty, name, description));
        }

        Presets = result;

        return result;
    }
}
