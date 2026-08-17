using System.Reflection;
using EFT.Ballistics;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using Systems.Effects;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch allows bots to hear if a bullet has impacted nearby
/// </summary>
public class BulletImpactPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EffectsCommutator), nameof(EffectsCommutator.PlayHitEffect));
    }

    [PatchPostfix]
    public static void PatchPostfix(Shot info)
    {
        if (BotManagerComponent.Instance != null)
        {
            BotManagerComponent.Instance.BotHearing.BulletImpacted(info);
        }
    }
}
