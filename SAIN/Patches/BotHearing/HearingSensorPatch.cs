using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch is used to disable BSG's bot hearing for a bot when using SAIN
/// </summary>
public class HearingSensorPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotHearingSensor), nameof(BotHearingSensor.Init));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotHearingSensor __instance)
    {
        if (SAINEnableClass.IsSAINDisabledForBot(__instance._botOwner))
        {
            return false;
        }
        return true;
    }
}
