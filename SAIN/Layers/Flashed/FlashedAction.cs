using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.Shared.GlobalSettings.Categories.Look;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Layers.Flashed;

internal class FlashedAction(BotOwner bot) : BotAction(bot, "Flashed"), IBotAction
{
    private const float SEARCH_MIN_TIME = 0.7f;
    private const float SEARCH_MAX_TIME = 1.6f;
    private const float TRACK_MIN_TIME = 0.8f;
    private const float TRACK_MAX_TIME = 1.6f;
    private const float FIRE_MIN_TIME = 1f;
    private const float FIRE_MAX_TIME = 2f;

    private const float SEARCH_TRAVEL_TIME = 0.7f;
    private const float TRACK_TRAVEL_TIME = 0.9f;
    private const float FIRE_TRAVEL_TIME = 0.3f;

    private const float SEARCH_DISTANCE = 8f;
    private const float SEARCH_RADIUS = 5f;
    private const float TRACK_RADIUS = 2.5f;
    private const float FIRE_RADIUS = 1.2f;

    private const float DRIFT_MIN_TIME = 1.5f;
    private const float DRIFT_MAX_TIME = 3.5f;
    private const float STILL_MIN_TIME = 0.4f;
    private const float STILL_MAX_TIME = 1.2f;
    private const float DRIFT_STEP_DISTANCE = 3.5f;
    private const float DRIFT_SAMPLE_RANGE = 2f;
    private const float DRIFT_REPATH_DISTANCE = 1.5f;

    private const float DRIFT_MAX_TURN = 50f;

    private const float CARRY_BURST_MIN_TIME = 0.5f;
    private const float CARRY_BURST_MAX_TIME = 1.3f;
    private const float CARRY_SPREAD = 3.5f;

    private float Disorientation
    {
        get { return Bot.Info.FileSettings.Look.FlashDisorientation; }
    }

    private static FlashbangSettings Settings
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.Look.Flashbang; }
    }

    private enum EFlashedBehaviour
    {
        Search,
        Track,
        BlindFire,
    }

    public override void Update(CustomLayer.ActionData data)
    {
        if (_behaviourEnd < Time.time && !CarryingBurst)
        {
            PickBehaviour();
        }

        UpdateDrift();
    }

    public override void OnSteeringTicked()
    {
        Vector3 aimPoint = CurrentAimPoint();
        Bot.Steering.LookToPoint(aimPoint);

        if (!CarryingBurst && _behaviour != EFlashedBehaviour.BlindFire)
        {
            return;
        }

        Enemy enemy = Bot.GoalEnemy;
        if (enemy == null)
        {
            return;
        }

        // The delay exists to stop a bot opening up the instant it is blinded, so it does not apply to
        // one that already had the trigger down.
        if (!CarryingBurst && !Bot.Flashed.BlindFireReady)
        {
            return;
        }

        Bot.ManualShoot.TryShoot(enemy, aimPoint, true, EShootReason.Blindfire);
    }

    private void PickBehaviour()
    {
        _behaviour = RollBehaviour();

        switch (_behaviour)
        {
            case EFlashedBehaviour.BlindFire:
                _behaviourEnd = Time.time + Random.Range(FIRE_MIN_TIME, FIRE_MAX_TIME);
                SetAimTarget(RememberedPoint() + RandomOffset(FIRE_RADIUS * Disorientation), FIRE_TRAVEL_TIME);
                return;

            case EFlashedBehaviour.Track:
                _behaviourEnd = Time.time + Random.Range(TRACK_MIN_TIME, TRACK_MAX_TIME);
                SetAimTarget(RememberedPoint() + RandomOffset(TRACK_RADIUS * Disorientation), TRACK_TRAVEL_TIME);
                return;

            default:
                _behaviourEnd = Time.time + Random.Range(SEARCH_MIN_TIME, SEARCH_MAX_TIME);
                SetAimTarget(SearchPoint(), SEARCH_TRAVEL_TIME);
                return;
        }
    }

    private EFlashedBehaviour RollBehaviour()
    {
        var settings = Settings;
        float roll = Random.Range(0f, 100f);
        if (roll < settings.SearchChance)
        {
            return EFlashedBehaviour.Search;
        }
        roll -= settings.SearchChance;
        if (roll < settings.TrackChance)
        {
            return EFlashedBehaviour.Track;
        }
        return EFlashedBehaviour.BlindFire;
    }

    private void SetAimTarget(Vector3 target, float travelTime)
    {
        _aimFrom = CurrentAimPoint();
        _aimTo = target;
        _aimTravelStart = Time.time;
        _aimTravelTime = Mathf.Max(travelTime, 0.01f);
    }

    private Vector3 CurrentAimPoint()
    {
        float progress = (Time.time - _aimTravelStart) / _aimTravelTime;
        if (progress >= 1f)
        {
            return _aimTo;
        }
        return Vector3.Lerp(_aimFrom, _aimTo, Mathf.SmoothStep(0f, 1f, progress));
    }

    private Vector3 RememberedPoint()
    {
        Vector3? remembered = Bot.Flashed.LastSeenEnemyPoint;
        if (remembered != null)
        {
            return remembered.Value;
        }

        Vector3? lastKnown = Bot.GoalEnemy?.LastKnownPosition;
        return lastKnown ?? SearchPoint();
    }

    private Vector3 SearchPoint()
    {
        return Bot.Position + (Bot.LookDirection.normalized * SEARCH_DISTANCE) + RandomOffset(SEARCH_RADIUS * Disorientation);
    }

    private static Vector3 RandomOffset(float radius)
    {
        Vector2 flat = Random.insideUnitCircle * radius;
        return new Vector3(flat.x, Random.Range(-radius, radius) * 0.4f, flat.y);
    }

    private void UpdateDrift()
    {
        if (!Settings.PanicMovement || CarryingBurst || _behaviour == EFlashedBehaviour.BlindFire)
        {
            if (_drifting)
            {
                _drifting = false;
                _driftDestination = null;
                Bot.Mover.Stop();
            }
            return;
        }

        if (_driftStateEnd < Time.time)
        {
            _drifting = !_drifting;
            _driftStateEnd =
                Time.time + (_drifting ? Random.Range(DRIFT_MIN_TIME, DRIFT_MAX_TIME) : Random.Range(STILL_MIN_TIME, STILL_MAX_TIME));
            if (!_drifting)
            {
                _driftDestination = null;
                Bot.Mover.Stop();
            }
        }

        if (!_drifting)
        {
            return;
        }

        if (
            _driftDestination == null
            || (Bot.Position - _driftDestination.Value).sqrMagnitude < DRIFT_REPATH_DISTANCE * DRIFT_REPATH_DISTANCE
        )
        {
            StepDriftHeading();
        }

        if (_driftDestination != null)
        {
            Bot.Mover.SetTargetMoveSpeed(Settings.PanicMovementSpeed);
            Bot.Mover.WalkToPoint(_driftDestination.Value, false);
        }
    }

    private void StepDriftHeading()
    {
        _driftHeading += Random.Range(-DRIFT_MAX_TURN, DRIFT_MAX_TURN);
        Vector3 direction = Quaternion.Euler(0f, _driftHeading, 0f) * Vector3.forward;
        _driftDestination = NavMeshHelpers.GetNearbyNavMeshPoint(Bot.Position + (direction * DRIFT_STEP_DISTANCE), DRIFT_SAMPLE_RANGE);

        if (_driftDestination == null)
        {
            _driftHeading += 180f;
        }
    }

    private bool CarryingBurst
    {
        get { return _carriedBurstEnd > Time.time; }
    }

    private float HeadingFromLook()
    {
        Vector3 look = Bot.LookDirection;
        look.y = 0f;
        return look.sqrMagnitude > 0.01f ? Quaternion.LookRotation(look).eulerAngles.y : Random.Range(0f, 360f);
    }

    public override void Start()
    {
        base.Start();
        _driftDestination = null;
        _drifting = false;
        _driftStateEnd = Time.time + Random.Range(STILL_MIN_TIME, STILL_MAX_TIME);
        _driftHeading = HeadingFromLook();

        Vector3 remembered = RememberedPoint();
        _aimFrom = remembered;
        _aimTo = remembered;
        _aimTravelStart = Time.time;
        _aimTravelTime = 0.01f;

        if (Bot.ManualShoot.Shooting)
        {
            // Already firing, so the aim starts on target and walks off it across the burst.
            float burst = Random.Range(CARRY_BURST_MIN_TIME, CARRY_BURST_MAX_TIME);
            _carriedBurstEnd = Time.time + burst;
            _behaviour = EFlashedBehaviour.BlindFire;
            _behaviourEnd = _carriedBurstEnd;
            SetAimTarget(remembered + RandomOffset(CARRY_SPREAD * Disorientation), burst);
            return;
        }

        _carriedBurstEnd = 0f;
        _behaviourEnd = 0f;
        PickBehaviour();
    }

    public override void Stop()
    {
        base.Stop();
        Bot.ManualShoot.Reset();
        Bot.Mover.SetTargetMoveSpeed(1f);
        _driftDestination = null;
        _carriedBurstEnd = 0f;
    }

    private EFlashedBehaviour _behaviour;
    private float _behaviourEnd;

    private bool _drifting;
    private float _driftStateEnd;
    private float _driftHeading;
    private Vector3? _driftDestination;

    private float _carriedBurstEnd;

    private Vector3 _aimFrom;
    private Vector3 _aimTo;
    private float _aimTravelStart;
    private float _aimTravelTime = 0.01f;
}
