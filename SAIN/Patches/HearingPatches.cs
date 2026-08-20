using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Preset.Shared.Enums;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.Hearing;

//Todo: Test all patches in this file and seperate them out under .BotHearing
//Testing should be done on Headless fika to make sure that they are fika compatible.

public class GrenadeCollisionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Grenade), nameof(Grenade.OnCollisionHandler));
    }

    [PatchPostfix]
    public static void Patch(Grenade __instance, SoundBank ____collisionSoundBank)
    {
        ____collisionSoundBank.Rolloff = _defaultRolloff * ROLLOFF_MULTI;
        //Logger.LogDebug($"Rolloff {_defaultRolloff} after {___soundBank_0.Rolloff}");
        BotManagerComponent.Instance?.GrenadeController.GrenadeCollided(__instance, 35);
    }

    private static float _defaultRolloff = 40;
    private const float ROLLOFF_MULTI = 1.5f;
}

public class GrenadeCollisionPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Throwable), nameof(Throwable.OnCollisionHandler));
    }

    [PatchPostfix]
    public static void Patch(ref float ___IgnoreCollisionTrackingTimer, Throwable __instance)
    {
        ___IgnoreCollisionTrackingTimer = Time.time + 0.35f;
    }
}

public class TryPlayShootSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(AIData), nameof(AIData.TryPlayShootSound));
    }

    [PatchPrefix]
    public static bool PatchPrefix(AIData __instance)
    {
        __instance.UseFlare = true;
        return false;
    }
}

public class OnWeaponModifiedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.WeaponModified));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player.FirearmController __instance, Player ____player)
    {
        if (GameWorldComponent.TryGetPlayerComponent(____player, out PlayerComponent PlayerComponent))
        {
            PlayerComponent.Equipment.WeaponModified(__instance.Weapon);
        }
    }
}

public class SoundClipNameCheckerPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseSoundPlayer), nameof(BaseSoundPlayer.SoundAtPointEventHandler));
    }

    [PatchPrefix]
    public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
    {
        if (soundName == FUSE)
        {
            BaseSoundPlayer.SoundElement soundElement = __instance.AdditionalSounds.Find(
                (BaseSoundPlayer.SoundElement elem) => elem.EventName == FUSE || elem.EventName == "Snd" + FUSE
            );
            if (soundElement != null)
            {
                soundElement.RollOff = 60;
                soundElement.Volume = 1;
            }
        }
    }

    private const string FUSE = "SndFuse";
}

/// <summary>
/// Replacement for <c>ToggleSoundPatch</c> targeting <c>Player.PlayToggleSound</c>
/// </summary>
public class TogglableSetPatch : ModulePatch
{
    private static readonly AccessTools.FieldRef<Player, Vector3> _speechLocalPosition = AccessTools.FieldRefAccess<Player, Vector3>(
        "SpeechLocalPosition"
    );

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(TogglableComponent), nameof(TogglableComponent.Set));
    }

    [PatchPrefix]
    public static void PatchPrefix(TogglableComponent __instance, bool value, bool simulate, bool silent)
    {
        if (Singleton<EFT.GameWorld>.Instance is HideoutGameWorld)
        {
            return;
        }

        if (!simulate && !silent && __instance.On != value && __instance.Item.Owner is Player.PlayerInventoryController inventoryController)
        {
            Player player = inventoryController.Player;

            const float baseRange = 10f;
            BotManagerComponent.Instance?.BotHearing.PlayAISound(
                player.ProfileId,
                SAINSoundType.GearSound,
                player.Position + _speechLocalPosition(player),
                baseRange,
                1f
            );
        }
    }
}

public class SpawnInHandsSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.SpawnInHands));
    }

    [PatchPostfix]
    public static void PatchPostfix(Player __instance, Item item)
    {
        //AudioClip itemClip = Singleton<GUISounds>.Instance.GetItemClip(item.ItemSound, EInventorySoundType.pickup);
        //if (itemClip != null) {
        //    SAINBotController.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GearSound, __instance.Position, 30f, 1f);
        //}
    }
}
