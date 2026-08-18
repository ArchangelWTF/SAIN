using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Models.Enums;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class GrenadeVelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }
    public float VelocityMagnitude { get; private set; }

    private const float GRENADE_UPDATE_FREQUENCY = 0.5f;

    public void Awake()
    {
        _grenade = this.GetComponent<Grenade>();
        _grenade.DestroyEvent += GrenadeDestroyed;
        _rigidBody = (Rigidbody)_rigidBodyField.GetValue(_grenade);
    }

    public void Update()
    {
        if (_grenade == null)
        {
            return;
        }

        if (_rigidBody == null)
        {
            GrenadeDestroyed(_grenade);
            return;
        }
        if (_nextUpdateTime < Time.time)
        {
            _nextUpdateTime = Time.time + GRENADE_UPDATE_FREQUENCY;
            Velocity = _rigidBody.velocity;
            VelocityMagnitude = Velocity.magnitude;
            //Logger.LogInfo($"Grenade {_grenade.Id} Velocity [{Velocity}] Magnitude: [{VelocityMagnitude}]");
        }
    }

    private Rigidbody _rigidBody;
    private Grenade _grenade;

    static GrenadeVelocityTracker()
    {
        _rigidBodyField = AccessTools.Field(typeof(Throwable), "Rigidbody");
    }

    private void GrenadeDestroyed(Throwable grenade)
    {
        if (grenade != null)
        {
            grenade.DestroyEvent -= GrenadeDestroyed;
        }
        Destroy(this);
    }

    private static FieldInfo _rigidBodyField;
    private float _nextUpdateTime;
}

public class GrenadeReactionClass : BotSubClass<BotGrenadeManager>, IBotClass
{
    /// <summary>
    /// Distance at which a grenade is tracked at all. Whether the bot reacts is governed separately
    /// by GRENADE_REACT_DISTANCE.
    /// </summary>
    private const float MAX_ENEMY_GRENADE_DIST_TOCARE = 125f;

    /// <summary>
    /// Distance at which a tracked grenade is worth reacting to.
    /// </summary>
    private const float GRENADE_REACT_DISTANCE = 18f;

    /// <summary>
    /// Beyond this range, cover between the bot and the blast is enough to justify holding position.
    /// Closer than this a frag still reaches around most things, so the bot moves regardless.
    /// </summary>
    private const float OCCLUSION_TRUST_DISTANCE = 7f;

    /// <summary>
    /// Inside this range the blast is the only thing that matters, so every personality breaks
    /// straight out of it. Pushing through a grenade at your feet is suicide, not aggression.
    /// </summary>
    private const float BLAST_OVERRIDE_DISTANCE = 6f;

    /// <summary>
    /// Angle squadmates are fanned apart by, so a group does not pile into one escape route.
    /// </summary>
    private const float SQUAD_FAN_ANGLE = 35f;

    /// <summary>
    /// Base chance a bot notices a given grenade, before difficulty scaling.
    /// </summary>
    private const float BASE_NOTICE_CHANCE = 0.8f;

    public GrenadeTrackerClass DangerGrenade { get; private set; }
    public Vector3? GrenadeDangerPoint
    {
        get { return DangerGrenade?.DangerPoint; }
    }

    public Dictionary<Throwable, GrenadeTrackerClass> EnemyGrenadesList { get; private set; } = [];

    public GrenadeReactionClass(BotGrenadeManager ThrowWeap)
        : base(ThrowWeap) { }

    public override void Init()
    {
        var grenadeController = BotManagerComponent.Instance.GrenadeController;
        grenadeController.OnGrenadeCollision += GrenadeCollision;
        grenadeController.OnGrenadeThrown += EnemyGrenadeThrown;
        grenadeController.OnGrenadeDangerUpdated += GrenadeDangerUpdated;
        base.Init();
    }

    public override void ManualUpdate()
    {
        foreach (var tracker in EnemyGrenadesList.Values)
        {
            tracker?.Update();
        }
        UpdateDangerGrenade();
        base.ManualUpdate();
    }

    private void UpdateDangerGrenade()
    {
        GrenadeTrackerClass closest = null;
        float closestSqrDist = GRENADE_REACT_DISTANCE * GRENADE_REACT_DISTANCE;
        Vector3 botPosition = Bot.Position;

        foreach (var tracker in EnemyGrenadesList.Values)
        {
            if (tracker?.Grenade == null || !tracker.CanReact)
            {
                continue;
            }
            float sqrDist = (tracker.DangerPoint - botPosition).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = tracker;
            }
        }

        if (closest == null)
        {
            DangerGrenade = null;
            Reaction = EGrenadeReaction.None;
            return;
        }

        if (DangerGrenade != closest)
        {
            DangerGrenade = closest;
            Reaction = GetReaction();
#if DEBUG
            if (SAINPlugin.DebugMode)
            {
                Logger.LogDebug($"[{Bot.name}] grenade reaction [{Reaction}] at [{Mathf.Sqrt(closestSqrDist)}m]");
            }
#endif
        }
    }

    public EGrenadeReaction Reaction { get; private set; }

    public bool ShallAvoidGrenade()
    {
        return DangerGrenade != null && Reaction != EGrenadeReaction.None;
    }

    /// <summary>
    /// Seconds before the current danger grenade is expected to go off, or zero when there is none.
    /// </summary>
    public float EstimatedTimeRemaining
    {
        get { return DangerGrenade?.EstimatedTimeRemaining ?? 0f; }
    }

    /// <summary>
    /// True when solid geometry sits between the bot and the blast. Fragments do not turn corners, so
    /// a bot already behind something has far less reason to abandon its position than the raw
    /// distance suggests.
    /// </summary>
    public bool BlastIsOccluded()
    {
        Vector3? danger = GrenadeDangerPoint;
        if (danger == null)
        {
            return false;
        }

        Vector3 from = Bot.Transform.WeaponRoot;
        Vector3 to = danger.Value + (Vector3.up * 0.1f);
        Vector3 direction = to - from;
        float distance = direction.magnitude;
        if (distance < 0.1f)
        {
            return false;
        }
        return Physics.Raycast(from, direction.normalized, distance, LayersMaskController.HighPolyWithTerrainMask);
    }

    private EGrenadeReaction GetReaction()
    {
        if (DangerGrenade.Grenade?.GrenadeSettings.CollisionSound == GrenadeSettings.CollisionSounds.smoke)
        {
            return EGrenadeReaction.None;
        }

        float distance = (Bot.Position - (GrenadeDangerPoint ?? Bot.Position)).magnitude;
        if (distance > OCCLUSION_TRUST_DISTANCE && BlastIsOccluded())
        {
            return EGrenadeReaction.None;
        }

        switch (Bot.Info.Personality)
        {
            case EPersonality.Wreckless:
            case EPersonality.GigaChad:
            case EPersonality.Chad:
                return Bot.GoalEnemy != null ? EGrenadeReaction.Push : EGrenadeReaction.Scatter;

            case EPersonality.Rat:
            case EPersonality.Coward:
            case EPersonality.Timmy:
                return EGrenadeReaction.Retreat;

            case EPersonality.SnappingTurtle:
                return EGrenadeReaction.Relocate;

            default:
                return EGrenadeReaction.Scatter;
        }
    }

    /// <summary>
    /// Direction the bot should break in, pointing away from the blast. Squadmates reacting to the
    /// same grenade are fanned apart so a group does not pile into one escape route and eat the next one.
    /// </summary>
    public Vector3 GetEscapeDirection()
    {
        Vector3 dangerPoint = GrenadeDangerPoint ?? Bot.Position;
        Vector3 away = Bot.Position - dangerPoint;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f)
        {
            // Standing on top of it, so any direction beats staying put.
            away = -Bot.LookDirection;
            away.y = 0f;
        }
        away = away.normalized;

        // Aggressive bots advance on the thrower, but only once clear of the blast, and only if the
        // thrower is not on the far side of the grenade.
        if (Reaction == EGrenadeReaction.Push && (Bot.Position - dangerPoint).sqrMagnitude > BLAST_OVERRIDE_DISTANCE * BLAST_OVERRIDE_DISTANCE)
        {
            Enemy goalEnemy = Bot.GoalEnemy;
            if (goalEnemy != null)
            {
                Vector3 toEnemy = goalEnemy.EnemyPosition - Bot.Position;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude > 0.01f)
                {
                    toEnemy = toEnemy.normalized;
                    if (Vector3.Dot(away, toEnemy) > 0f)
                    {
                        away = (away + toEnemy).normalized;
                    }
                }
            }
        }

        int fanIndex = GetSquadFanIndex();
        if (fanIndex == 0)
        {
            return away;
        }

        // Alternate left/right of the escape vector, widening with each additional member.
        float angle = SQUAD_FAN_ANGLE * ((fanIndex + 1) / 2) * (fanIndex % 2 == 0 ? 1f : -1f);
        return Quaternion.Euler(0f, angle, 0f) * away;
    }

    /// <summary>
    /// Stable per-bot slot within its squad, so the fan direction does not flip between frames.
    /// </summary>
    private int GetSquadFanIndex()
    {
        var squad = Bot.Squad;
        if (squad?.BotInGroup != true)
        {
            return 0;
        }

        int index = 0;
        string myId = Bot.ProfileId;
        foreach (string memberId in squad.Members.Keys)
        {
            if (memberId == myId)
            {
                return index;
            }
            index++;
        }
        return 0;
    }

    public override void Dispose()
    {
        var grenadeController = BotManagerComponent.Instance.GrenadeController;
        grenadeController.OnGrenadeCollision -= GrenadeCollision;
        grenadeController.OnGrenadeThrown -= EnemyGrenadeThrown;
        grenadeController.OnGrenadeDangerUpdated -= GrenadeDangerUpdated;

        foreach (var tracker in EnemyGrenadesList.Values)
        {
            if (tracker?.Grenade != null)
            {
                tracker.Grenade.DestroyEvent -= RemoveGrenade;
            }
        }

        EnemyGrenadesList.Clear();
        base.Dispose();
    }

    public void EnemyGrenadeThrown(Grenade grenade, Vector3 dangerPoint, string profileId)
    {
        if (Bot == null || profileId == Bot.ProfileId || !Bot.BotActive)
        {
            return;
        }

        Enemy enemy = Bot.EnemyController.GetEnemy(profileId, false);
        bool throwerIsKnownAndClose = enemy != null && enemy.RealDistance <= MAX_ENEMY_GRENADE_DIST_TOCARE;
        bool landingNearMe = (dangerPoint - Bot.Position).sqrMagnitude <= MAX_ENEMY_GRENADE_DIST_TOCARE * MAX_ENEMY_GRENADE_DIST_TOCARE;

        if (!EnemyGrenadesList.ContainsKey(grenade) && (throwerIsKnownAndClose || landingNearMe))
        {
            EnemyGrenadesList.Add(grenade, new GrenadeTrackerClass(Bot, grenade, dangerPoint, GetReactionTime(), RollWillNotice()));
            grenade.DestroyEvent += RemoveGrenade;
            return;
        }
        BotOwner.BewareGrenade.AddGrenadeDanger(dangerPoint, grenade);
    }

    private void GrenadeCollision(Grenade grenade, float maxRange)
    {
        if (EnemyGrenadesList.TryGetValue(grenade, out var Tracker))
        {
            Tracker?.CheckHeardGrenadeCollision(maxRange);
        }
    }

    private void GrenadeDangerUpdated(Grenade grenade, Vector3 Danger)
    {
        if (EnemyGrenadesList.TryGetValue(grenade, out var Tracker))
        {
            Tracker.UpdateGrenadeDanger(Danger);
        }
    }

    private void RemoveGrenade(Throwable grenade)
    {
        if (grenade != null)
        {
            grenade.DestroyEvent -= RemoveGrenade;
            EnemyGrenadesList.Remove(grenade);
        }
    }

    private bool RollWillNotice()
    {
        float chance = Mathf.Clamp01(BASE_NOTICE_CHANCE * Bot.Info.Profile.DifficultyModifier);
        return Random.value <= chance;
    }

    public float GetReactionTime()
    {
        float reactionTime = 0.25f;
        reactionTime /= Bot.Info.Profile.DifficultyModifier;
        reactionTime *= Random.Range(0.75f, 1.25f);
        return Mathf.Clamp(reactionTime, 0.2f, 1f);
    }
}
