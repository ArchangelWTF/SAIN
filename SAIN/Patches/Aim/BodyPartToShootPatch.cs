using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.Shared.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
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

            if (TrySelectorPart(__instance, bot, ref __result))
            {
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

    private static bool TrySelectorPart(EnemyInfo enemyInfo, BotComponent bot, ref Vector3 result)
    {
        Enemy enemy = bot.EnemyController.GetEnemy(enemyInfo.Person.ProfileId, false);
        Vector3? point = enemy?.AimTarget.GetPointToShoot(allowRepick: false);
        if (point == null)
        {
            return false;
        }

        if (
            enemy.AimTarget.ChosenPart is EAimTargetPart chosen
            && enemyInfo._allParts.TryGetValue(chosen.ToBodyPartType(), out EnemyPart part)
        )
        {
            enemyInfo.LastPartToShoot = part;
        }

        result = point.Value;
        return true;
    }
}
