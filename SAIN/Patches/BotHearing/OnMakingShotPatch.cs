using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SPT.Reflection.Patching;
using UnityEngine;
using EFT.InventoryLogic;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch allows bots to hear whenever a player shoots their gun
/// </summary>
public class OnMakingShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.OnMakingShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, IWeapon weapon, Vector3 force)
    {
        if (GameWorldComponent.TryGetPlayerComponent(__instance, out PlayerComponent PlayerComponent))
        {
            PlayerComponent.OnMakingShot(weapon, force);
        }
    }
}
