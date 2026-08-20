using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.Shared.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo;

internal class AvoidGrenadeAction(BotOwner bot) : BotAction(bot, "Avoid Grenade"), IBotAction
{
    private const float RETREAT_DISTANCE = 22f;
    private const float SCATTER_DISTANCE = 14f;
    private const float COVER_DOT_THRESHOLD = 0.25f;
    private const int ESCAPE_DISTANCE_STEPS = 3;
    private const float BLAST_URGENT_DISTANCE = 8f;
    private const float FAIL_LOG_INTERVAL = 2f;

    /// <summary>
    /// Rough sprint speed, used only to decide whether an escape is worth attempting at all.
    /// </summary>
    private const float SPRINT_SPEED_ESTIMATE = 5f;

    /// <summary>
    /// Ceiling on how long one escape destination is committed to.
    /// </summary>
    private const float DESTINATION_COMMIT_TIME = 1.5f;

    /// <summary>
    /// Distance from the committed destination that counts as arrived.
    /// </summary>
    private const float ARRIVED_DISTANCE = 2f;

    /// <summary>
    /// How far the grenade must move before an escape already underway is worth abandoning.
    /// </summary>
    private const float DANGER_MOVED_DISTANCE = 4f;

    /// <summary>
    /// Bearings tried either side of the escape vector, since the ideal one often faces a wall.
    /// </summary>
    private static readonly float[] ESCAPE_ANGLES = [0f, 40f, -40f, 80f, -80f, 130f, -130f];

    public override void Update(CustomLayer.ActionData data)
    {
        var reactions = Bot.Grenade.GrenadeReactionClass;
        Vector3? dangerPoint = reactions.GrenadeDangerPoint;
        if (dangerPoint == null)
        {
            return;
        }

        if (!ShallPickNewDestination(dangerPoint.Value))
        {
            return;
        }

        _committedDanger = dangerPoint.Value;
        _nextRepathTime = Time.time + DESTINATION_COMMIT_TIME;

        EGrenadeReaction reaction = reactions.Reaction;
        Vector3 escapeDirection = reactions.GetEscapeDirection();
        float distanceToBlast = (Bot.Position - dangerPoint.Value).magnitude;
        bool blastIsImminent = distanceToBlast < BLAST_URGENT_DISTANCE;

        float metresStillNeeded = BLAST_URGENT_DISTANCE - distanceToBlast;
        if (metresStillNeeded > 0f && reactions.EstimatedTimeRemaining * SPRINT_SPEED_ESTIMATE < metresStillNeeded)
        {
            Bot.Mover.Prone.SetProne(true);
            _destination = null;
            return;
        }

        bool sprint = blastIsImminent || reaction == EGrenadeReaction.Retreat || reaction == EGrenadeReaction.Scatter;

        if (!blastIsImminent)
        {
            float minDistance = reaction == EGrenadeReaction.Relocate ? 4f : 8f;
            CoverPoint point = Bot.Cover.FindPointInDirection(-escapeDirection, COVER_DOT_THRESHOLD, minDistance);
            if (point != null && Bot.Mover.GoToCoverPoint(point, sprint, sprint ? ESprintUrgency.High : ESprintUrgency.Low))
            {
                _destination = point.Position;
                return;
            }
        }

        float baseDistance = reaction == EGrenadeReaction.Retreat ? RETREAT_DISTANCE : SCATTER_DISTANCE;
        for (int step = 0; step < ESCAPE_DISTANCE_STEPS; step++)
        {
            float distance = baseDistance * (1f - (step * 0.33f));
            for (int i = 0; i < ESCAPE_ANGLES.Length; i++)
            {
                Vector3 direction = Quaternion.Euler(0f, ESCAPE_ANGLES[i], 0f) * escapeDirection;
                Vector3? sampled = NavMeshHelpers.GetNearbyNavMeshPoint(Bot.Position + (direction * distance), 3f);
                if (sampled == null)
                {
                    continue;
                }

                bool moving = sprint
                    ? Bot.Mover.RunToPoint(sampled.Value, false, -1, ESprintUrgency.High)
                    : Bot.Mover.WalkToPoint(sampled.Value, false);
                if (moving)
                {
                    _destination = sampled.Value;
                    return;
                }
            }
        }

        _destination = null;
#if DEBUG
        if (SAINPlugin.DebugMode && _nextFailLogTime < Time.time)
        {
            _nextFailLogTime = Time.time + FAIL_LOG_INTERVAL;
            Logger.LogWarning($"[{Bot.name}] could not find anywhere to go to avoid grenade");
        }
#endif
    }

    private bool ShallPickNewDestination(Vector3 dangerPoint)
    {
        if (_destination == null)
        {
            return true;
        }
        if ((dangerPoint - _committedDanger).sqrMagnitude > DANGER_MOVED_DISTANCE * DANGER_MOVED_DISTANCE)
        {
            return true;
        }
        if ((Bot.Position - _destination.Value).sqrMagnitude < ARRIVED_DISTANCE * ARRIVED_DISTANCE)
        {
            return true;
        }
        return _nextRepathTime < Time.time;
    }

    public override void OnSteeringTicked()
    {
        var reactions = Bot.Grenade.GrenadeReactionClass;
        Vector3? danger = reactions.GrenadeDangerPoint;

        if (danger != null && (Bot.Position - danger.Value).sqrMagnitude < BLAST_URGENT_DISTANCE * BLAST_URGENT_DISTANCE)
        {
            Bot.Steering.LookToMovingDirection();
            return;
        }

        Enemy enemy = Bot.GoalEnemy;

        if (Shoot.ShootAnyVisibleEnemies(enemy))
        {
            return;
        }

        if (reactions.Reaction == EGrenadeReaction.Push && Bot.Steering.SteerByPriority(enemy, false))
        {
            return;
        }

        Bot.Steering.LookToMovingDirection();
    }

    public override void Start()
    {
        base.Start();
        _nextRepathTime = 0f;
        _destination = null;
    }

    public override void Stop()
    {
        base.Stop();
        _destination = null;
        Bot.Mover.Prone.SetProne(false);
        Bot.Mover.SetTargetMoveSpeed(1f);
    }

    private float _nextFailLogTime;
    private float _nextRepathTime;
    private Vector3? _destination;
    private Vector3 _committedDanger;
}
