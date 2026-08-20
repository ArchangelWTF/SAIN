using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.Shared.Enums;

namespace SAIN.SAINComponent.Classes.Info;

public class WeaponInfo
{
    public WeaponInfo(Weapon weapon)
    {
        Weapon = weapon;
        WeaponClass = TryGetWeaponClass(weapon);
        AmmoCaliber = TryGetAmmoCaliber(weapon);
        UpdateSettings(SAINPresetClass.Instance);
        PresetHandler.OnPresetUpdated += UpdateSettings;
    }

    public EWeaponClass WeaponClass { get; private set; }
    public string AmmoCaliber { get; private set; }
    public float CalculatedAudibleRange { get; private set; }
    public AISoundType AISoundType
    {
        get { return HasSuppressor ? AISoundType.silencedGun : AISoundType.gun; }
    }

    public SAINSoundType SoundType
    {
        get { return HasSuppressor ? SAINSoundType.SuppressedShot : SAINSoundType.Shot; }
    }

    public bool HasRedDot
    {
        get { return RedDot != null; }
    }

    public bool HasOptic
    {
        get { return Optic != null; }
    }

    public bool HasSuppressor { get; private set; }
    public float BaseAudibleRange { get; private set; } = 150f;
    public float MuzzleLoudness { get; private set; } = 0f;
    public bool Subsonic
    {
        get { return Weapon != null && BulletSpeed < SuperSonicSpeed; }
    }

    public Mod FlashHider { get; private set; }
    public Mod Suppressor { get; private set; }
    public Mod RedDot { get; private set; }
    public Mod Optic { get; private set; }
    public float BulletSpeed { get; private set; } = 600f;
    public float EngagementDistance { get; private set; } = 150f;
    public Weapon Weapon { get; private set; }
    public float Durability
    {
        get { return Weapon.Repairable.Durability / (float)Weapon.Repairable.TemplateDurability; }
    }

    private void UpdateSettings(SAINPresetClass preset)
    {
        if (preset.GlobalSettings.Shoot.EngagementDistance.TryGetValue(WeaponClass, out float distance))
        {
            EngagementDistance = distance;
        }
        if (preset.GlobalSettings.Hearing.HearingDistances.TryGetValue(AmmoCaliber, out float range))
        {
            BaseAudibleRange = range;
        }
    }

    public void Update(Player Player)
    {
        if (Weapon != null)
        {
            UpdateWeaponData(Player, Weapon.Mods);
        }
    }

    private void UpdateWeaponData(Player Player, IEnumerable<Mod> mods)
    {
        FindModType(mods);
        MuzzleLoudness = 0f;
        MuzzleLoudness += Suppressor?.Loudness ?? 0f;
        MuzzleLoudness += FlashHider?.Loudness ?? 0f;
        BulletSpeed = Weapon.CurrentAmmoTemplate.InitialSpeed * Weapon.SpeedFactor;
        CalculatedAudibleRange = BaseAudibleRange + MuzzleLoudness;
        if (Suppressor != null || _suppressedWeapons.Contains(Weapon.TemplateId._stringID))
        {
            CalculatedAudibleRange *= SuppressorModifier(BulletSpeed);
            HasSuppressor = true;
        }
        else
        {
            HasSuppressor = false;
        }
        //Log();
    }

    public void WeaponEquiped(Player player)
    {
        Update(player);
    }

    public void WeaponModified(Player player)
    {
        Update(player);
    }

    public void Dispose()
    {
        PresetHandler.OnPresetUpdated -= UpdateSettings;
    }

    private void FindModType(IEnumerable<Mod> mods)
    {
        RedDot = null;
        Optic = null;
        FlashHider = null;
        Suppressor = null;
        if (mods != null)
        {
            foreach (Mod mod in mods)
            {
                var modType = CheckItemType(mod.GetType());
                switch (modType)
                {
                    case EModType.RedDot:
                        RedDot ??= mod;
                        break;
                    case EModType.Optic:
                        Optic ??= mod;
                        break;
                    case EModType.FlashHider:
                        FlashHider ??= mod;
                        break;
                    case EModType.Suppressor:
                        Suppressor ??= mod;
                        break;
                    default:
                        if (Suppressor == null && _suppressorMods.Contains(mod.TemplateId._stringID))
                        {
                            Suppressor = mod;
                        }
                        break;
                }

                if (RedDot != null && Optic != null && FlashHider != null && Suppressor != null)
                {
                    return;
                }
            }
        }
    }

    private static float SuppressorModifier(float bulletspeed)
    {
        if (bulletspeed < SuperSonicSpeed)
        {
            return SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SubsonicModifier;
        }
        return SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SuppressorModifier;
    }

    private static EModType CheckItemType(Type type)
    {
        if (CheckTemplateType(type, _flashHiderTypeId))
        {
            return EModType.FlashHider;
        }
        if (CheckTemplateType(type, _suppressorTypeId))
        {
            return EModType.Suppressor;
        }
        for (int i = 0; i < _redDotTypes.Length; i++)
        {
            if (CheckTemplateType(type, _redDotTypes[i]))
            {
                return EModType.RedDot;
            }
        }
        for (int i = 0; i < _opticTypes.Length; i++)
        {
            if (CheckTemplateType(type, _opticTypes[i]))
            {
                return EModType.Optic;
            }
        }
        return EModType.None;
    }

    private static bool CheckTemplateType(Type modType, string id)
    {
        if (JsonTypes.TypeTable.TryGetValue(id, out Type result) && result == modType)
        {
            return true;
        }
        if (JsonTypes.TemplateTypeTable.TryGetValue(id, out result) && result == modType)
        {
            return true;
        }
        return false;
    }

    private static EWeaponClass TryGetWeaponClass(Weapon weapon)
    {
        EWeaponClass WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.Template.weapClass);
        if (WeaponClass == default)
        {
            WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.WeapClass);
        }
        return WeaponClass;
    }

    private static string TryGetAmmoCaliber(Weapon weapon)
    {
        string caliber = weapon.Template.ammoCaliber;
        if (string.IsNullOrEmpty(caliber))
        {
            caliber = weapon.AmmoCaliber;
        }
        return caliber;
    }

    private void Log()
    {
        Logger.LogDebug(
            $"Found Weapon Info: "
                + $"Weapon: [{Weapon.ShortName.Localized()}] "
                + $"Weapon Class: [{WeaponClass}] "
                + $"Ammo Caliber: [{AmmoCaliber}] "
                + $"Calculated Audible Range: [{CalculatedAudibleRange}] "
                + $"Base Audible Range: [{BaseAudibleRange}] "
                + $"Muzzle Loudness: [{MuzzleLoudness}] "
                + $"Speed Factor: [{Weapon.SpeedFactor}] "
                + $"Subsonic: [{Subsonic}] "
                + $"Has Red Dot? [{HasRedDot}] "
                + $"Has Optic? [{HasOptic}] "
                + $"Has Suppressor? [{HasSuppressor}]"
        );

        Logger.LogDebug(
            $"Found Weapon Info (continue):"
                + $" Suppressor: [{Suppressor?.ShortName.Localized()}]"
                + $" Flash Hider: [{FlashHider?.ShortName.Localized()}]"
                + $" Optic: [{Optic?.ShortName.Localized()}]"
                + $" Red dot: [{RedDot?.ShortName.Localized()}]"
        );
    }

    private const float SuperSonicSpeed = 343.2f;

    // Contrary to the name, Muzzle here means "Muzzle Device", i.e., the parent of flash hiders and suppressors.
    // Muzzle brake is also considered a "FlashHider".
    //private static readonly string MuzzleTypeId = "5448fe394bdc2d0d028b456c";
    private static readonly string _flashHiderTypeId = "550aa4bf4bdc2dd6348b456b";
    private static readonly string _suppressorTypeId = "550aa4cd4bdc2dd8348b456c";

    private static readonly string[] _opticTypes =
    {
        "55818add4bdc2d5b648b456f", // AssaultScopeTypeId
        "55818ae44bdc2dde698b456c", // OpticScopeTypeId
        "55818aeb4bdc2ddc698b456a", // SpecialScopeTypeId
    };
    private static readonly string[] _redDotTypes =
    {
        "55818ad54bdc2ddc698b4569", // CollimatorTypeId
        "55818acf4bdc2dde698b456b", // CompactCollimatorTypeId
    };

    // Some weapons don't have their suppressors listed in their mod lists, eventhough they are suppressed.
    // So, we check for their weapon IDs instead.
    private static readonly string[] _suppressedWeapons =
    {
        "674d6121c09f69dfb201a888", // Aklys Defense Velociraptor .300
    };

    // Some mods are suppressors, but aren't classified as suppressors.
    private static readonly string[] _suppressorMods =
    {
        "5888945a2459774bf43ba385", // DVL-10 suppressed barrel
    };
}

public enum EModType
{
    None,
    Suppressor,
    RedDot,
    Optic,
    FlashHider,
}
