using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

[DataContract]
public class PowerCalcSettings : SAINSettingsBase<PowerCalcSettings>, ISAINSettings
{
    [DataMember]
    [Name("PMC Power")]
    [Description("Add X points to a bot's power level if they are a PMC")]
    [Category("Bot Type Power Value")]
    [MinMax(-100, 100, 10)]
    public float PMC_POWER = 20f;

    [DataMember]
    [Name("Scav Power")]
    [Description("Add X points to a bot's power level if they are a Scav")]
    [Category("Bot Type Power Value")]
    [MinMax(-100, 100, 10)]
    public float SCAV_POWER = -20f;

    [DataMember]
    [Name("Shotgun Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SHOTGUN_POWER = 40f;

    [DataMember]
    [Name("Smg Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SMG_POWER = 75f;

    [DataMember]
    [Name("Assault Carbine Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float ASSAULT_CARBINE_POWER = 60f;

    [DataMember]
    [Name("Assault Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float ASSAULT_RIFLE_POWER = 45f;

    [DataMember]
    [Name("Machinegun Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float MG_POWER = 55f;

    [DataMember]
    [Name("Sniper Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SNIPE_POWER = -30f;

    [DataMember]
    [Name("Marksman Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float MARKSMAN_RIFLE_POWER = 10f;

    [DataMember]
    [Name("Pistol Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float PISTOL_POWER = -10f;

    [DataMember]
    [Name("Red Dot / 1x Holo Sight Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float RED_DOT_POWER = 30f;

    [DataMember]
    [Name("Magnified Optic Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float OPTIC_POWER = -20f;

    [DataMember]
    [Name("Suppressor Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float SUPPRESSOR_POWER = 20f;

    [DataMember]
    [Name("Body Armor Class Power")]
    [Description("For each AC level, add X to a bot's power level. So if they have level 4 armor, add this value 4 times.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float ARMOR_CLASS_COEF = 30f;

    [DataMember]
    [Name("Body Armor Class Power - Realism Mod")]
    [Description(
        "If Realism Mod is loaded, use this AC Power value. "
            + "For each AC level, add X to a bot's power level. So if they have level 4 armor, add this value 4 times."
    )]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float ARMOR_CLASS_COEF_REALISM = 20f;

    [DataMember]
    [Name("Helmet Class Power")]
    [Description("If a bot has an armored helmet above class 1, but lower than 5, add X to thier power level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float HELMET_POWER = 30f;

    [DataMember]
    [Name("Heavy Helmet Class Power")]
    [Description("If a bot has an armored helmet above class 4, add X to thier power level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float HELMET_HEAVY_POWER = 60f;

    [DataMember]
    [Name("Faceshield Power")]
    [Description("If a bot has an armored face shield, add X to thier Power Level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float FACESHIELD_POWER = 20f;

    [DataMember]
    [Name("Headphones Power")]
    [Description("If a bot has headphones, add X to thier Power Level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float EARPRO_POWER = 20f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
