using SAIN.Preset.Shared;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GearStealthValues;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;
using SAIN.Preset.Shared.Preset;

namespace SAINServerMod.Models.Preset;

public sealed record GeneratedPreset(
    SAINPresetDefinition Info,
    GlobalSettingsClass GlobalSettings,
    Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> BotSettings,
    Dictionary<EPersonality, PersonalitySettingsClass> Personalities,
    Dictionary<EEquipmentType, List<ItemStealthValue>> GearStealthValues
)
{
    public SAINPresetBundle ToBundle()
    {
        return new SAINPresetBundle
        {
            Info = Info,
            GlobalSettings = GlobalSettings,
            BotSettings = BotSettings,
            Personalities = Personalities,
            GearStealthValues = GearStealthValues,
        };
    }
}
