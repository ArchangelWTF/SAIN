using System;
using SAIN.Extensions;
using SAIN.Plugin;
using SAIN.Preset.BotSettings;
using SAIN.Preset.GearStealthValues;
using SAIN.Preset.Personalities;
using SAIN.Preset.Server;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Preset;

namespace SAIN.Preset;

public class SAINPresetClass
{
    public static SAINPresetClass Instance { get; private set; }
    public SAINPresetDefinition Info { get; private set; }
    public GlobalSettingsClass GlobalSettings { get; private set; }
    public SAINBotSettingsClass BotSettings { get; private set; }
    public PersonalityManagerClass PersonalityManager { get; private set; }
    public GearStealthValuesClass GearStealthValuesClass { get; private set; }

    public SAINPresetClass(SAINPresetBundle bundle)
    {
        Instance = this;
        if (bundle.Info.IsCustom)
        {
            SAINPlugin.EditorDefaults.SelectedDefaultPreset = SAINDifficulty.none;
            SAINPlugin.EditorDefaults.SelectedCustomPreset = bundle.Info.Name;
        }
        else
        {
            SAINPlugin.EditorDefaults.SelectedCustomPreset = string.Empty;
            SAINPlugin.EditorDefaults.SelectedDefaultPreset = bundle.Info.BaseSAINDifficulty;
        }

        Info = bundle.Info;
        GlobalSettings = bundle.GlobalSettings ?? new GlobalSettingsClass();

        var serverSettings = new System.Collections.Generic.Dictionary<EFT.WildSpawnType, SAINSettingsGroupClass>();
        if (bundle.BotSettings != null)
        {
            foreach (var kv in bundle.BotSettings)
            {
                serverSettings[kv.Key.ToEft()] = kv.Value;
            }
        }

        BotSettings = new SAINBotSettingsClass(this, serverSettings);
        PersonalityManager = new PersonalityManagerClass(this, bundle.Personalities);
        GearStealthValuesClass = new GearStealthValuesClass(bundle.GearStealthValues);
    }

    public void Init()
    {
        GlobalSettings.Init();
        BotSettings.Init();
        PersonalityManager.Init();
    }

    public void UpdateDefaults(SAINPresetClass preset = null)
    {
        GlobalSettings.UpdateDefaults(preset?.GlobalSettings);
        BotSettings.UpdateDefaults(preset?.BotSettings);
        PersonalityManager.UpdateDefaults(preset?.PersonalityManager);
    }

    public static void ExportAll(SAINPresetClass preset)
    {
        if (!PresetHandler.CanEditCurrentPreset)
        {
            Logger.LogWarning("[SAIN] Preset editing is locked by the server. Your changes were not saved.");
            return;
        }

        ConfigEditingTracker.Clear();

        if (preset.Info.IsCustom == false)
        {
            SAINPresetDefinition newPreset = preset.Info.Clone();

            newPreset.Name += " [Modified]";
            newPreset.Creator = "user";
            newPreset.Description = "[Modified] " + newPreset.Description;
            newPreset.DateCreated = DateTime.Today.ToString();

            PresetHandler.SavePresetDefinition(newPreset);
            PresetHandler.InitPresetFromDefinition(newPreset, true);
            PresetHandler.UpdateExistingBots();
            return;
        }

        PresetSync.PushCustomPreset(preset);
        PresetHandler.UpdateExistingBots();
    }
}

public abstract class BasePreset
{
    public BasePreset(SAINPresetClass presetClass)
    {
        Preset = presetClass;
        Info = presetClass.Info;
    }

    public readonly SAINPresetClass Preset;
    public readonly SAINPresetDefinition Info;
}
