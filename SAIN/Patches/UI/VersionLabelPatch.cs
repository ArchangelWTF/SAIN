using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SAIN.Patches.UI;

public class VersionLabelPatch : ModulePatch
{
    private const string ModName = "SAIN";

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(PreloaderUI), nameof(PreloaderUI.RefreshCornerLabel));
    }

    [PatchPrefix]
    public static void Prefix(ref string ___string_2, ref LocalizedText ____alphaVersionLabel)
    {
        if (!___string_2.Contains(ModName, System.StringComparison.OrdinalIgnoreCase))
        {
            ___string_2 += $" | {ModName} {AssemblyInfoClass.SAINVersion}";

            ____alphaVersionLabel.LocalizationKey = ___string_2;
        }
    }
}
