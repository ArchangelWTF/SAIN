using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.GlobalSettings.Categories.General;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.Vision;

public class UpdateLightEnablePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.UpdateLightEnable));
    }

    [PatchPrefix]
    public static bool PatchPrefix(float curLightDist, ref float __result, BotLight __instance)
    {
        __result = curLightDist;
        if (SAINEnableClass.GetSAIN(__instance._owner.ProfileId, out var flashedBot) && flashedBot.Flashed.IsFlashed)
        {
            return false;
        }
        if (!__instance._owner)
        {
            return false;
        }
        __instance._curLightDist = curLightDist;

        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        var lookSettings = GlobalSettingsClass.Instance.Look.Light;
        float turnOnRatio = lookSettings.LightOnRatio;
        float turnOffRatio = lookSettings.LightOffRatio;

        bool isOn = __instance.IsEnable;
        bool wantOn = !isOn && timeModifier <= turnOnRatio && __instance._owner.Memory.IsPeace;
        bool wantOff = isOn && timeModifier >= turnOffRatio;
        __instance._canUseNow = timeModifier < turnOffRatio;

        if (wantOn)
        {
            try
            {
                __instance.TurnOn(true);
            }
            catch { }
        }
        if (wantOff)
        {
            try
            {
                __instance.TurnOff(true, true);
            }
            catch { }
#if DEBUG
            try
            {
                __instance.TurnOff(true, true);
            }
            catch (Exception e)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogError(e);
                }
            }
#endif

            if (__instance.IsEnable)
            {
                var gameworld = GameWorldComponent.Instance;
                if (gameworld == null)
                {
#if DEBUG
                    Logger.LogError($"GameWorldComponent is null, cannot check if bot has flashlight on!");
#endif
                    return false;
                }
                PlayerComponent playerComponent = gameworld.PlayerTracker.GetPlayerComponent(__instance._owner.ProfileId);
                if (playerComponent == null)
                {
#if DEBUG
                    Logger.LogError($"Player Component is null, cannot check if bot has flashlight on!");
#endif
                    return false;
                }
                if (playerComponent.Flashlight.WhiteLight || (__instance._owner.NightVision.UsingNow && playerComponent.Flashlight.IRLight))
                {
                    float min = __instance._owner.Settings.FileSettings.Look.VISIBLE_DISNACE_WITH_LIGHT;
                    __result = Mathf.Clamp(curLightDist, min, float.MaxValue);
                }
            }
        }
        return false;
    }
}

public class UpdateLightEnablePatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.LeaveDarkPlace));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotLight __instance)
    {
        if (!__instance.IsEnable)
        {
            return false;
        }
        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        float turnOffRatio = GlobalSettingsClass.Instance.Look.Light.LightOffRatio;
        bool wantOff = timeModifier >= turnOffRatio;
        if (wantOff)
        {
            __instance.TurnOff(true, true);
        }
        return false;
    }
}

public class ToggleNightVisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotNightVisionData), nameof(BotNightVisionData.CheckWhatIWant));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotNightVisionData __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance._owner.ProfileId, out var flashedBot) && flashedBot.Flashed.IsFlashed)
        {
            return false;
        }

        float timeModifier = BotManagerComponent.Instance.TimeVision.TimeVisionDistanceModifier;
        var lookSettings = GlobalSettingsClass.Instance.Look.Light;
        float turnOnRatio = lookSettings.NightVisionOnRatio;
        float turnOffRatio = lookSettings.NightVisionOffRatio;

        if (__instance._nightVisionAtPocket)
        {
            if (timeModifier < turnOnRatio)
            {
                __instance.MoveToHeadAndToggleOn();
                return false;
            }
        }
        else
        {
            if (timeModifier < turnOnRatio)
            {
                __instance.ToggleOnIfNeed();
            }
            if (timeModifier >= turnOffRatio)
            {
                __instance.MoveToHeadPocket();
            }
        }
        return false;
    }
}

/// <summary>
/// Disable the ai task registration of SAIN bots for vision updates.
/// </summary>
public class DisableLookUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LookSensor), nameof(LookSensor.Activate));
    }

    [PatchPrefix]
    public static bool Patch(LookSensor __instance)
    {
        if (SAINEnableClass.IsBotExcluded(__instance._botOwner))
        {
            return true;
        }

        __instance.CalcVisibleDistance();
        return false;
    }
}

public class GlobalLookSettingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotGlobalLookData), nameof(BotGlobalLookData.Update));
    }

    [PatchPostfix]
    public static void Patch(BotGlobalLookData __instance)
    {
        __instance.CHECK_HEAD_ANY_DIST = true;
        __instance.MIDDLE_DIST_CAN_SHOOT_HEAD = true;
        __instance.SHOOT_FROM_EYES = false;
    }
}

public class NoAIESPPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(BotOwner).GetMethod(
            nameof(BotOwner.IsEnemyLookingAtMe),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [typeof(IPlayer)],
            null
        );
    }

    [PatchPrefix]
    public static bool PatchPrefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}

public class BotLightTurnOnPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLight), nameof(BotLight.TurnOn));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotLight __instance)
    {
        if (__instance._isInDarkPlace && !SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight.AllowLightOnForDarkBuildings)
        {
            __instance._isInDarkPlace = false;
        }
        if (__instance._isInDarkPlace || __instance._owner.Memory.GoalEnemy != null)
        {
            return true;
        }
        if (!ShallTurnLightOff(__instance._owner.Profile.Info.Settings.Role))
        {
            return true;
        }
        __instance._owner.BotLight.TurnOff(false, true);
        return false;
    }

    private static bool ShallTurnLightOff(WildSpawnType wildSpawnType)
    {
        FlashlightSettings settings = SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight;
        if (EnumValues.WildSpawn.IsScav(wildSpawnType))
        {
            return settings.TurnLightOffNoEnemySCAV;
        }
        if (wildSpawnType.IsPmcBot())
        {
            return settings.TurnLightOffNoEnemyPMC;
        }
        if (EnumValues.WildSpawn.IsGoons(wildSpawnType))
        {
            return settings.TurnLightOffNoEnemyGOONS;
        }
        if (wildSpawnType.IsBoss())
        {
            return settings.TurnLightOffNoEnemyBOSS;
        }
        if (wildSpawnType.IsFollower())
        {
            return settings.TurnLightOffNoEnemyFOLLOWER;
        }
        return settings.TurnLightOffNoEnemyRAIDERROGUE;
    }
}

public class FlashbangDurationPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotFlashGrenade), nameof(BotFlashGrenade.AddBlindEffect));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotFlashGrenade __instance, float time, Vector3 position)
    {
        if (!SAINEnableClass.GetSAIN(__instance._owner.ProfileId, out var bot) || bot.Flashed == null)
        {
            return true;
        }

        bot.Flashed.ApplyFlash(time, position);
        return false;
    }
}

public class VisionSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.GetVisibilityChangeSpeedK));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref float __result, EnemyInfo __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(__instance.ProfileId, false);
            enemy ??= sain.EnemyController.CheckAddEnemy(__instance.Person);
            if (enemy != null)
            {
                float sainMod = EnemyGainSightClass.GetGainSightModifier(enemy);
                __result /= sainMod;
                enemy.Vision.LastGainSightResult = __result;
            }
        }
    }
}

public class WeatherVisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.GetWeatherK));
    }

    [PatchPrefix]
    public static bool PatchPrefix(EnemyInfo __instance, ref float __result)
    {
        if (SAINEnableClass.IsBotExcluded(__instance.Owner))
        {
            return true;
        }

        __result = 1f;
        return false;
    }
}

public class IsPointInVisibleSectorCallerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyPartVision), nameof(EnemyPartVision.CheckLineOfSight));
    }

    [PatchPrefix]
    public static bool PatchPrefix(EnemyPartVision __instance, BotOwner owner, EnemyPart part)
    {
        if (SAINEnableClass.GetSAIN(owner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(part.EnemyPlayer.ProfileId, false);
            if (enemy != null)
            {
                if (enemy.Vision.Angles.CanBeSeen && enemy.Vision.EnemyParts.CanBeSeen)
                {
                    // Allow original method to run, which ends up checking LookSensor.IsPointInVisibleSector next
                    return true;
                }
                else
                {
                    // Exit method early, set BotToTargetHit and HasLineOfSight back to default
                    __instance.BotToTargetHit = default;
                    __instance.HasLineOfSight = false;
                    return false;
                }
            }
        }

        // Run original method if SAIN is not enabled
        return true;
    }
}

public class IsPointInVisibleSectorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LookSensor), nameof(LookSensor.IsPointInVisibleSector));
    }

    [PatchPrefix]
    public static bool PatchPrefix(LookSensor __instance, ref bool __result)
    {
        // We already executed this check in the patch above, just need to patch this method to complete early.
        if (SAINEnableClass.GetSAIN(__instance._botOwner.ProfileId, out var _))
        {
            __result = true;
            return false;
        }

        // Run original method if SAIN is not enabled
        return true;
    }
}

public class VisionDistancePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.GetAdditionalSensorDistance));
    }

    [PatchPostfix]
    public static void PatchPrefix(ref float __result, EnemyInfo __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var sain))
        {
            Enemy enemy = sain.EnemyController.GetEnemy(__instance.ProfileId, false);
            if (enemy != null)
            {
                __result += enemy.Vision._visionDistance.Value;
            }
        }
    }
}

public class CheckFlashlightPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.SetLightsState));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player ____player)
    {
        PlayerComponent playerComponent = GameWorldComponent.Instance?.PlayerTracker.GetPlayerComponent(____player?.ProfileId);
        if (playerComponent != null)
        {
            BotManagerComponent.Instance.BotHearing.PlayAISound(
                playerComponent,
                SAINSoundType.GearSound,
                playerComponent.Player.WeaponRoot.position,
                35f,
                1f,
                true
            );
            var flashLight = playerComponent.Flashlight;
            flashLight.CheckDevice();

            if (!flashLight.WhiteLight && !flashLight.Laser)
            {
                (____player.AIData as AIData).UsingLight = false;
            }
        }
    }
}
