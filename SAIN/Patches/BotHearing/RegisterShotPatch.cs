using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using EFT.Ballistics;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch enables registering shots in the game world, allowing bots to track if bullets flew by them
/// </summary>
public class RegisterShotPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.RegisterShot));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player ____player, Item weapon, Shot shot)
    {
        GameWorldComponent.Instance?.RegisterShot(____player, shot, weapon);
    }
}
