using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Extensions;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.Generic.Fixes;

internal class RunToEnemyUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMeleeWeaponData), nameof(BotMeleeWeaponData.RunToEnemyUpdate));
    }

    [PatchPrefix]
    public static bool Patch(BotMeleeWeaponData __instance)
    {
        if (SAINEnableClass.GetSAIN(__instance._owner.ProfileId, out BotComponent bot) && bot.SAINLayersActive)
        {
            Enemy enemy = bot.GoalEnemy;
            if (enemy == null)
            {
                return false;
            }
            __instance.ShallEndRun = false;
            if (!__instance._owner.WeaponManager.IsMelee)
            {
                if (!__instance._owner.WeaponManager.Selector.CanChangeToMeleeWeapons)
                {
                    __instance.ShallEndRun = true;
                    return false;
                }
                __instance._owner.WeaponManager.Selector.ChangeToMelee();
            }
            if (__instance._owner.BotLay.IsLay)
            {
                __instance._owner.BotLay.GetUp(false);
            }
            bot.Mover.SetTargetPose(1f);
            EnemyInfo goalEnemy = enemy.EnemyInfo;
            bool flag;
            if (flag = (goalEnemy.Distance < __instance.DIST_TO_HIT))
            {
                bot.Steering.LookToPoint(goalEnemy.GetBodyPartPosition());
                if (goalEnemy.Person.AIData.Player.MovementContext.IsInPronePose)
                {
                    bot.Mover.SetTargetPose(0f);
                }
            }
            else
            {
                bot.Steering.LookToMovingDirection(true);
            }

            if (__instance.Running && goalEnemy.Distance > __instance.DIST_TO_STOP_SPRINT)
            {
                bot.Mover.ActivePath?.RequestStartSprint(ESprintUrgency.High, "melee");
            }
            if (__instance._nextTryHitTime < Time.time)
            {
                __instance.ResetHitTime((flag && __instance.TryHit(goalEnemy)) ? 10f : __instance.TRY_HIT_PERIOD_FALSE);
            }
            if (bot.Mover.Running)
            {
                if (__instance._runPathCheck < Time.time)
                {
                    float num;
                    if (__instance._useZigZag)
                    {
                        num = (
                            (goalEnemy.Distance > __instance.FAR_DIST)
                                ? __instance.farRecalc
                                : ((goalEnemy.Distance > __instance.MID_DIST) ? __instance.midRecalcZZ : __instance.closeRecalcZZ)
                        );
                    }
                    else
                    {
                        num =
                            (goalEnemy.Distance > __instance.FAR_DIST)
                                ? __instance.farRecalc
                                : ((goalEnemy.Distance > __instance.MID_DIST) ? __instance.midRecalc : __instance.closeRecalc);
                    }
                    __instance._runPathCheck = Time.time + num;
                    if (!__instance.CanRunToEnemyToHit(goalEnemy, out Vector3[] way))
                    {
                        __instance.ShallEndRun = true;
                        return false;
                    }
                    if (goalEnemy.Distance < __instance._owner.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
                    {
                        bot.Mover.ActivePath.Cancel(0.1f);
                    }
                    else
                    {
                        bot.Mover.RunToPoint(goalEnemy.CurrPosition, true, -1, ESprintUrgency.High, true);
                    }
                }
            }
            else
            {
                if (!__instance.CanRunToEnemyToHit(goalEnemy, out Vector3[] way2))
                {
                    __instance.ShallEndRun = true;
                    return false;
                }
                if (goalEnemy.Distance < __instance._owner.Settings.FileSettings.Shoot.MELEE_STOP_MOVE_DISTANCE)
                {
                    bot.Mover.ActivePath?.Cancel(0.1f);
                }
                else
                {
                    bot.Mover.RunToPoint(goalEnemy.CurrPosition, true, -1, ESprintUrgency.High, true);
                }
            }
            return false;
        }
        return false;
    }
}

internal class EnableVaultPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.InitVaultingComponent));
    }

    [PatchPrefix]
    public static void Patch(Player __instance, ref bool aiControlled)
    {
        if (__instance.UsedSimplifiedSkeleton)
        {
            return;
        }

        aiControlled = false;
    }
}

internal class FightShallReloadFixPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotReload), nameof(BotReload.FightShallReload));
    }

    [PatchPrefix]
    public static bool Patch(BotReload __instance, ref bool __result)
    {
        if (SAINEnableClass.IsBotInCombat(__instance._owner))
        {
            __result = true;
            return false;
        }
        return true;
    }
}

//Todo: Still necessary?
internal class FixItemTakerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotItemTaker), nameof(BotItemTaker.OnThrowItem));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotItemTaker __instance)
    {
        return __instance._owner.IsBotNotNullOrDead();
    }
}

//Todo: Still necessary?
internal class FixItemTakerPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotItemTaker), nameof(BotItemTaker.RefreshClosestItems));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotItemTaker __instance)
    {
        return __instance._owner.IsBotNotNullOrDead();
    }
}

internal class RotateClampPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.Rotate));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, ref bool ignoreClamp)
    {
        if (__instance?.IsAI == true && __instance.IsSprintEnabled && SAINEnableClass.IsBotInCombat(__instance))
        {
            ignoreClamp = true;
        }
    }
}

internal class BotGroupAddEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddEnemy));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IPlayer person)
    {
        if (person == null || person.IsAI && person.AIData?.BotOwner?.GetPlayer == null)
        {
            return false;
        }

        return true;
    }
}

internal class BotMemoryAddEnemyPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMemory), nameof(BotMemory.AddEnemy));
    }

    [PatchPrefix]
    public static bool PatchPrefix(IPlayer enemy)
    {
        if (enemy == null || enemy.IsAI && enemy.AIData?.BotOwner?.GetPlayer == null)
        {
            return false;
        }

        return true;
    }
}

public class StopSetToNavMeshPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMover), nameof(BotMover.SetPosition));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotMover __instance)
    {
        if (SAINEnableClass.IsBotInCombat(__instance._owner))
        {
            __instance.PositionOnWayInner = __instance._owner.Position;
            __instance._owner.Mover.LocalAvoidance.DropOffset();
            return false;
        }
        return true;
    }
}

public class StopSetToNavMeshPatch2 : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotMoverImpostor), nameof(BotMoverImpostor.OnMotionApplied));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotMoverImpostor __instance)
    {
        if (SAINEnableClass.IsBotInCombat(__instance._owner))
        {
            return false;
        }
        return true;
    }
}
