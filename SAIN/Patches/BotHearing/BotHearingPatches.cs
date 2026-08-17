using System.Reflection;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using SAIN.Preset.Shared.Enums;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.BotHearing;

// This set of patches are all for allowing bots to hear various sounds
// If they are in this file they have all been tested on Fika and work as intended for both host and clients
// As well as just working on base SPT

public class TreeSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TreeInteractive), nameof(TreeInteractive.PlaySoundBank));
    }

    [PatchPostfix]
    public static void Patch(Vector3 soundPosition, BetterSource source, IObserverToPlayerBridge player, SoundBank ____soundBank)
    {
        if (player.iPlayer != null)
        {
            float baseRange = 50f;

            if (____soundBank != null)
            {
                baseRange = ____soundBank.Rolloff * player.SoundRadius * 0.8f;
            }

            BotManagerComponent.Instance?.BotHearing.PlayAISound(
                player.iPlayer.ProfileId,
                SAINSoundType.Bush,
                soundPosition,
                baseRange,
                1f
            );
        }
    }
}

public class DoorSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Door), nameof(Door.Interact), [typeof(InteractionResult)]);
    }

    [PatchPrefix]
    public static void PatchPrefix(Door __instance, InteractionResult interactionResult)
    {
        switch (interactionResult.InteractionType)
        {
            case EInteractionType.Open:
                float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.DOOR_OPEN_SOUND_RANGE;
                BotManagerComponent.Instance?.BotHearing.PlayAISound(
                    __instance.InteractingPlayer.ProfileId,
                    SAINSoundType.Door,
                    __instance.InteractingPlayer.Position,
                    baseRange,
                    1f
                );
                break;
        }
    }
}

public class DoorBreachSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.PlayBreachSound));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.DOOR_KICK_SOUND_RANGE;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(____player.ProfileId, SAINSoundType.Door, ____player.Position, baseRange, 1f);
    }
}

public class DryShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.DryShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_DryFire;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(
            ____player.ProfileId,
            SAINSoundType.DryFire,
            ____player.WeaponRoot.position,
            baseRange,
            1f
        );
    }
}

public class PlaySwitchHeadlightSoundPatch : ModulePatch
{
    // Taken from the Player class, this never changes
    private static Vector3 _speechLocalPosition = new(0f, 1.2f, 0f);

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.SetLightsState));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref bool __result, Player ____player)
    {
        if (__result)
        {
            BotManagerComponent.Instance?.BotHearing.PlayAISound(
                ____player.ProfileId,
                SAINSoundType.GearSound,
                ____player.Position + _speechLocalPosition,
                10f,
                1f
            );
        }
    }
}

public class LootingSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(LootableContainer), nameof(LootableContainer.Interact), [typeof(InteractionResult)]);
    }

    [PatchPostfix]
    public static void PatchPostfix(LootableContainer __instance, InteractionResult interactionResult)
    {
        switch (interactionResult.InteractionType)
        {
            case EInteractionType.Open:
                float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Looting;
                BotManagerComponent.Instance?.BotHearing.PlayAISound(
                    __instance.InteractingPlayer.ProfileId,
                    SAINSoundType.Looting,
                    __instance.InteractingPlayer.Position,
                    baseRange,
                    1f
                );
                break;
        }
    }
}

public class AimSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.PlayAimingSound));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player.FirearmController __instance, Player ____player)
    {
        float baseRange = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_AimingandGearRattle;
        BotManagerComponent.Instance?.BotHearing.PlayAISound(
            ____player.ProfileId,
            SAINSoundType.GearSound,
            ____player.Position,
            baseRange,
            __instance.CalculateAimingSoundVolume()
        );
    }
}
