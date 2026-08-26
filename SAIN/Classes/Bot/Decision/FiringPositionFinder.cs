using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Decision;

public class FiringPositionFinder(BotComponent bot) : BotBase(bot)
{
    public Vector3? Position { get; private set; }

    private const float SEARCH_INTERVAL = 2f;
    private const float POSITION_LIFETIME = 20f;
    private const float ARRIVE_DISTANCE = 2f;
    private const int DIRECTION_COUNT = 12;
    private const float ANGLE_STEP = 360f / DIRECTION_COUNT;
    private const float NAV_SAMPLE_RANGE = 2f;
    private const float EFFECTIVE_RANGE_COEF = 1.25f;

    private static readonly float[] _positionRadius = [10f, 20f, 32f];

    private float _nextSearchTime;
    private float _positionSetTime;

    public bool Find(Enemy enemy)
    {
        EnemyPlace lastKnown = enemy?.KnownPlaces.LastKnownPlace;
        if (lastKnown == null)
        {
            Clear();
            return false;
        }

        if (KeepCurrentPosition())
        {
            return true;
        }

        if (_nextSearchTime > Time.time)
        {
            return false;
        }
        _nextSearchTime = Time.time + SEARCH_INTERVAL;

        Position = Search(lastKnown);
        _positionSetTime = Time.time;
        return Position != null;
    }

    public void Clear()
    {
        Position = null;
    }

    private bool KeepCurrentPosition()
    {
        if (Position == null)
        {
            return false;
        }
        if (Time.time - _positionSetTime > POSITION_LIFETIME)
        {
            return false;
        }
        return (Position.Value - Bot.Position).sqrMagnitude > ARRIVE_DISTANCE * ARRIVE_DISTANCE;
    }

    private Vector3? Search(EnemyPlace lastKnown)
    {
        Vector3 target = lastKnown.EnemyHeadAtPosition();
        Vector3 botPosition = Bot.Position;
        float maxShootDistance = Bot.Info.WeaponInfo.EffectiveWeaponDistance * EFFECTIVE_RANGE_COEF;

        // No candidate can be in range if even the closest possible one is outside it.
        if ((target - botPosition).magnitude - _positionRadius[_positionRadius.Length - 1] > maxShootDistance)
        {
            return null;
        }

        Vector3 eyeOffset = Bot.Transform.EyePosition - botPosition;
        Vector3? best = null;
        float bestDistance = float.MaxValue;

        for (int radiusIndex = 0; radiusIndex < _positionRadius.Length; radiusIndex++)
        {
            float radius = _positionRadius[radiusIndex];
            for (int i = 0; i < DIRECTION_COUNT; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, i * ANGLE_STEP, 0f) * Vector3.forward * radius;
                if (!NavMesh.SamplePosition(botPosition + offset, out NavMeshHit navHit, NAV_SAMPLE_RANGE, NavMesh.AllAreas))
                {
                    continue;
                }

                float distanceFromBot = (navHit.position - botPosition).magnitude;
                if (distanceFromBot >= bestDistance)
                {
                    continue;
                }

                Vector3 shootFrom = navHit.position + eyeOffset;
                Vector3 toTarget = target - shootFrom;
                float distanceToTarget = toTarget.magnitude;
                if (distanceToTarget > maxShootDistance)
                {
                    continue;
                }

                if (Physics.Raycast(shootFrom, toTarget, distanceToTarget, LayersMaskController.HighPolyWithTerrainMaskAI))
                {
                    continue;
                }

                if (!Bot.Mover.CanGoToPoint(navHit.position, out _, true))
                {
                    continue;
                }

                best = navHit.position;
                bestDistance = distanceFromBot;
            }
        }

        return best;
    }
}
