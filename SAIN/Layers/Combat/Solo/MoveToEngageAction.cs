using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo;

internal class MoveToEngageAction(BotOwner bot) : BotAction(bot, nameof(MoveToEngageAction)), IBotAction
{
    private const float RECALC_PATH_INTERVAL = 2f;
    private const float SPRINT_MIN_DISTANCE = 15f;

    private float _recalcPathTime;

    public override void Update(CustomLayer.ActionData data)
    {
        Enemy enemy = Bot.GoalEnemy;
        if (enemy == null)
        {
            Bot.Steering.SteerByPriority();
            return;
        }

        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.SetTargetMoveSpeed(1f);

        if (enemy.IsVisible && Shoot.ShootAnyVisibleEnemies(enemy))
        {
            if (Bot.Mover.Moving)
            {
                Bot.Mover.Stop();
            }
            Bot.Steering.SteerByPriority(enemy);
            return;
        }

        Vector3? firingPosition = Bot.Decision.EnemyDecisions.FiringPosition;
        if (firingPosition == null)
        {
            Bot.Steering.SteerByPriority(enemy);
            return;
        }

        if (_recalcPathTime > Time.time)
        {
            return;
        }
        _recalcPathTime = Time.time + RECALC_PATH_INTERVAL;

        Vector3 destination = firingPosition.Value;
        bool sprint = !BotOwner.Memory.IsUnderFire && (destination - Bot.Position).magnitude > SPRINT_MIN_DISTANCE;
        if (sprint && Bot.Mover.RunToPoint(destination, true, -1f, ESprintUrgency.Middle))
        {
            return;
        }
        Bot.Mover.WalkToPoint(destination, true);
    }

    public override void OnSteeringTicked()
    {
        Enemy enemy = Bot.GoalEnemy;
        if (TryShootAnyTarget(enemy))
        {
            Bot.Steering.SteerByPriority(enemy, false);
            return;
        }
        // Face where the shot is expected to come from once in position, otherwise watch the route.
        if (Bot.Mover.Moving && Bot.Steering.LookToMovingDirection())
        {
            return;
        }
        Bot.Steering.LookToLastKnownEnemyPosition(enemy);
    }
}
