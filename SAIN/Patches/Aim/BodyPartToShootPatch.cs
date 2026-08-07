using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.Aim;

public class BodyPartToShootPatch : ModulePatch
{
    private static readonly HashSet<BodyPartType> _nonHeadshotBodyPartTypes =
    [
        BodyPartType.body,
        BodyPartType.leftLeg,
        BodyPartType.rightLeg,
    ];

    private static readonly HashSet<BodyPartType> _upperBodyPartTypes =
    [
        BodyPartType.head,
        BodyPartType.rightArm,
        BodyPartType.leftArm,
        BodyPartType.body,
    ];

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.GetVisiblePartToShoot));
    }

    [PatchPrefix]
    public static bool Patch(ref Vector3 __result, EnemyInfo __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out BotComponent bot))
        {
            if (__instance.Owner.WeaponManager.UnderbarrelLauncherController.IsActive)
            {
                __result = __instance.CurrPosition;
                return false;
            }

            __instance._activeParts = _nonHeadshotBodyPartTypes;

            var aim = bot.Info.FileSettings.Aiming;
            var canBeHead = EFTMath.RandomBool(aim.AimForHeadChance) && aim.AimForHead;

            if (canBeHead)
            {
                __instance._activeParts = _upperBodyPartTypes;
            }

            __instance.FindLastPartRnd(true, canBeHead);

            if (__instance.LastPartToShoot == null)
            {
                __result = Vector3.zero;
                return false;
            }

            __result = __instance.LastPartToShoot.GetPartPositionWithOffset();
            return false;
        }

        return true;
    }
}
