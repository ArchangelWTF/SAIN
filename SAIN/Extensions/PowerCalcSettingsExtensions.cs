using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings.Categories;
using SAIN.SAINComponent.Classes.Info;

namespace SAIN.Extensions;

public static class PowerCalcSettingsExtensions
{
    private static readonly List<ArmorComponent> _armorComponents = [];

    private static readonly List<WildSpawnType> _pmcs = [WildSpawnType.pmcUSEC, WildSpawnType.pmcBEAR];

    private static readonly List<WildSpawnType> _scavs =
    [
        WildSpawnType.assault,
        WildSpawnType.cursedAssault,
        WildSpawnType.assaultGroup,
        WildSpawnType.crazyAssaultEvent,
        WildSpawnType.marksman,
    ];

    public static bool CalcPower(this PowerCalcSettings settings, PlayerComponent playerComponent, out float power)
    {
        power = 0f;
        if (playerComponent == null)
        {
            return false;
        }

        power += settings.WeaponPower(playerComponent);
        if (power == 0f)
        {
            return false;
        }

        power += settings.RolePower(playerComponent.Player.Profile.Info.Settings.Role);
        power += settings.ArmorPower(playerComponent.Player);

        if (playerComponent.Player.AIData is AIData aiData)
        {
            aiData.PowerOfEquipment = power;
        }

        return true;
    }

    private static float RolePower(this PowerCalcSettings settings, WildSpawnType type)
    {
        if (_pmcs.Contains(type))
        {
            return settings.PMC_POWER;
        }
        else if (_scavs.Contains(type))
        {
            return settings.SCAV_POWER;
        }
        return 0f;
    }

    private static float WeaponPower(this PowerCalcSettings settings, PlayerComponent player)
    {
        float result = 0f;

        WeaponInfo weaponInfo = player.Equipment.CurrentWeaponInfo ?? player.Equipment.WeaponInInventory;
        if (weaponInfo == null)
        {
            return 1f;
        }

        if (weaponInfo.HasSuppressor)
        {
            result += settings.SUPPRESSOR_POWER;
        }
        if (weaponInfo.HasRedDot)
        {
            result += settings.RED_DOT_POWER;
        }
        if (weaponInfo.HasOptic)
        {
            result += settings.OPTIC_POWER;
        }

        switch (weaponInfo.WeaponClass)
        {
            case EWeaponClass.pistol:
                result += settings.PISTOL_POWER;
                break;

            case EWeaponClass.smg:
                result += settings.SMG_POWER;
                break;

            case EWeaponClass.assaultCarbine:
                result += settings.ASSAULT_CARBINE_POWER;
                break;

            case EWeaponClass.assaultRifle:
                result += settings.ASSAULT_RIFLE_POWER;
                break;

            case EWeaponClass.machinegun:
                result += settings.MG_POWER;
                break;

            case EWeaponClass.marksmanRifle:
                result += settings.MARKSMAN_RIFLE_POWER;
                break;

            case EWeaponClass.sniperRifle:
                result += settings.SNIPE_POWER;
                break;

            case EWeaponClass.shotgun:
                result += settings.SHOTGUN_POWER;
                break;

            default:
                break;
        }
        return result;
    }

    private static float ArmorPower(this PowerCalcSettings settings, Player player)
    {
        _armorComponents.Clear();
        float result = 0;

        var inventory = player.Inventory;
        if (inventory == null)
        {
            return result;
        }
        var equipment = inventory.Equipment;
        if (equipment == null)
        {
            return result;
        }

        var armorVest = GetEquippedItem(equipment, EquipmentSlot.ArmorVest);
        if (armorVest != null)
        {
            armorVest.GetItemComponentsInChildrenNonAlloc(_armorComponents, true);
            float highestArmorClass = FindHighestArmorClass(_armorComponents);
            result += highestArmorClass * settings.ArmorClassCoef();
            _armorComponents.Clear();
        }
        else
        {
            var rig = GetEquippedItem(equipment, EquipmentSlot.TacticalVest);
            if (rig != null)
            {
                rig.GetItemComponentsInChildrenNonAlloc(_armorComponents, true);
                if (_armorComponents.Count > 0)
                {
                    float highestArmorClass = FindHighestArmorClass(_armorComponents);
                    result += highestArmorClass * settings.ArmorClassCoef();
                    _armorComponents.Clear();
                }
            }
        }

        var helmet = GetEquippedItem(equipment, EquipmentSlot.Headwear);
        if (helmet != null)
        {
            helmet.GetItemComponentsInChildrenNonAlloc(_armorComponents, true);
            if (_armorComponents.Count > 0)
            {
                float highestArmorClass = FindHighestArmorClass(_armorComponents);
                if (highestArmorClass > 4)
                {
                    result += settings.HELMET_HEAVY_POWER;
                }
                else if (highestArmorClass > 1)
                {
                    result += settings.HELMET_POWER;
                }
                _armorComponents.Clear();
            }
        }

        var faceProtection = GetEquippedItem(equipment, EquipmentSlot.FaceCover);
        if (faceProtection != null)
        {
            faceProtection.GetItemComponentsInChildrenNonAlloc(_armorComponents, true);
            if (_armorComponents.Count > 0)
            {
                result += settings.FACESHIELD_POWER;
            }
        }

        var earPro = GetEquippedItem(equipment, EquipmentSlot.Earpiece);
        if (earPro != null)
        {
            result += settings.EARPRO_POWER;
        }

        _armorComponents.Clear();
        return result;
    }

    private static Item GetEquippedItem(InventoryEquipment equipment, EquipmentSlot slot)
    {
        var container = equipment.GetSlot(slot);
        if (container == null)
        {
            return null;
        }
        return container.ContainedItem;
    }

    private static float FindHighestArmorClass(List<ArmorComponent> armorComponents)
    {
        float result = 0f;
        foreach (var armorComponent in armorComponents)
        {
            float armorClass = armorComponent.ArmorClass;
            if (armorClass > result)
            {
                result = armorClass;
            }
        }
        return result;
    }

    private static float ArmorClassCoef(this PowerCalcSettings settings)
    {
        if (ModDetection.RealismLoaded)
        {
            return settings.ARMOR_CLASS_COEF_REALISM;
        }
        return settings.ARMOR_CLASS_COEF;
    }
}
