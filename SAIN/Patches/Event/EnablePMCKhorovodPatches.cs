using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SAIN.Patches.Event;

/// <summary>
/// This patch enables a 'joke' that allows PMC's to come to the christmas tree when it is activated
/// </summary>
public class EnableUsecPmcKhorovodPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PmcUsecLayersStrategy), nameof(PmcUsecLayersStrategy.EventsPriority));
    }

    [PatchPrefix]
    public static bool Patch(ref BotEventsPriority __result)
    {
        if (!SAINPlugin.LoadedPreset.GlobalSettings.General.Jokes.EnableKhorovodPMCs)
        {
            return true;
        }

        __result = new BotEventsPriority(77, 74, 55, 75, -1, 76);

        return false;
    }
}

/// <summary>
/// This patch enables a 'joke' that allows PMC's to come to the christmas tree when it is activated
/// </summary>
public class EnableBearPmcKhorovodPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PmcBearLayersStrategy), nameof(PmcBearLayersStrategy.EventsPriority));
    }

    [PatchPrefix]
    public static bool Patch(ref BotEventsPriority __result)
    {
        if (!SAINPlugin.LoadedPreset.GlobalSettings.General.Jokes.EnableKhorovodPMCs)
        {
            return true;
        }

        __result = new BotEventsPriority(77, 74, 55, 75, -1, 76);

        return false;
    }
}
