using System.Collections.Generic;
using SAIN.Models.Preset.Personalities;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;

namespace SAIN.Preset.Personalities;

public class PersonalityManagerClass : BasePreset
{
    public PersonalityDictionary PersonalityDictionary = [];

    public PersonalityManagerClass(SAINPresetClass preset, Dictionary<EPersonality, PersonalitySettingsClass> serverPersonalities)
        : base(preset)
    {
        if (serverPersonalities == null || serverPersonalities.Count == 0)
        {
            Logger.LogError("[SAIN] Server preset contained no personalities.");
            return;
        }
        foreach (var kv in serverPersonalities)
        {
            PersonalityDictionary[kv.Key] = kv.Value;
        }
    }

    public void Init()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Init();
        }
    }

    public void UpdateDefaults(PersonalityManagerClass replacementClass = null)
    {
        foreach (var settings in PersonalityDictionary)
        {
            var replacementSettings = replacementClass?.PersonalityDictionary[settings.Key];
            settings.Value.UpdateDefaults(replacementSettings);
        }
    }

    public void Update()
    {
        foreach (var settings in PersonalityDictionary.Values)
        {
            settings.Update();
        }
    }
}
