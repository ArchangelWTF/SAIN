using System.Collections.Generic;
using EFT;
using SAIN.Extensions;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.BotSettings;

public class SAINBotSettingsClass(SAINPresetClass preset, Dictionary<WildSpawnType, SAINSettingsGroupClass> serverSettings)
    : BasePreset(preset)
{
    public Dictionary<WildSpawnType, SAINSettingsGroupClass> SAINSettings { get; private set; } = serverSettings;

    public void Update()
    {
        foreach (var settings in SAINSettings)
        {
            foreach (var group in settings.Value.Settings)
            {
                group.Value.Update();
            }
        }
    }

    public void Init()
    {
        foreach (var settings in SAINSettings)
        {
            foreach (var group in settings.Value.Settings)
            {
                group.Value.Init();
            }
        }
    }

    public void UpdateDefaults(SAINBotSettingsClass replacement)
    {
        foreach (var settings in SAINSettings)
        {
            SAINSettingsGroupClass replacementSettings = null;
            replacement?.SAINSettings.TryGetValue(settings.Key, out replacementSettings);
            foreach (var group in settings.Value.Settings)
            {
                SAINSettingsClass replacementGroup = null;
                replacementSettings?.Settings.TryGetValue(group.Key, out replacementGroup);
                group.Value.UpdateDefaults(replacementGroup);
            }
        }
    }

    public SAINSettingsClass GetSAINSettings(WildSpawnType type, BotDifficulty difficulty)
    {
        if (SAINSettings.TryGetValue(type, out var settingsGroup))
        {
            if (settingsGroup.Settings.TryGetValue(difficulty.ToESain(), out var settings))
            {
                return settings;
            }
            Logger.LogError($"[{difficulty}] does not exist in [{type}] SAIN Settings!");
        }
        else
        {
            Logger.LogError($"[{type}] does not exist in SAINSettings Dictionary!");
        }

        // Fall back to a bot type the server always sends, rather than throwing.
        if (
            SAINSettings.TryGetValue(WildSpawnType.pmcUSEC, out var fallbackGroup)
            && fallbackGroup.Settings.TryGetValue(ESainBotDifficulty.normal, out var fallback)
        )
        {
            return fallback;
        }
        return null;
    }
}
