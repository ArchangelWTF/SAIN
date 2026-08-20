using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;
using SAINServerMod.Extensions;

namespace SAINServerMod.Generators;

public static class PersonalityGenerator
{
    public static Dictionary<EPersonality, PersonalitySettingsClass> BuildDefaults()
    {
        var personalities = new Dictionary<EPersonality, PersonalitySettingsClass>();
        personalities.AddWreckless();
        personalities.AddSnappingTurtle();
        personalities.AddGigaChad();
        personalities.AddChad();
        personalities.AddRat();
        personalities.AddTimmy();
        personalities.AddCoward();
        personalities.AddNormal();
        return personalities;
    }
}
