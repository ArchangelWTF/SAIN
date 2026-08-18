using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents;

public class GrenadeTrackerClass
{
    private const float ESTIMATED_FUSE_TIME = 3.5f;
    private const float NOTICE_WITHOUT_LOOKING_DISTANCE = 3f;
    private const float SPOTTED_ON_ARRIVAL_DISTANCE = 10f;

    public GrenadeTrackerClass(BotComponent bot, Grenade grenade, Vector3 dangerPoint, float reactionTime, bool willNotice)
    {
        _bot = bot;
        _reactionTime = reactionTime;
        WillNotice = willNotice;
        DangerPoint = dangerPoint;
        Grenade = grenade;
        TimeThrown = Time.time;
        if ((grenade.transform.position - bot.Position).magnitude < SPOTTED_ON_ARRIVAL_DISTANCE)
        {
            SetSpotted();
        }
    }

    /// <summary>
    /// Rolled once when the grenade is thrown. A bot that fails the roll never reacts to this one, so
    /// grenades still land on people instead of every bot dodging every throw.
    /// </summary>
    public bool WillNotice { get; }

    public float TimeThrown { get; }

    public float TimeSinceThrown
    {
        get { return Time.time - TimeThrown; }
    }

    /// <summary>
    /// Estimated seconds before this goes off. Can be wrong for a cooked grenade, on purpose.
    /// </summary>
    public float EstimatedTimeRemaining
    {
        get { return Mathf.Max(0f, ESTIMATED_FUSE_TIME - TimeSinceThrown); }
    }

    public void CheckHeardGrenadeCollision(float maxRange)
    {
        if (Spotted)
        {
            return;
        }
        maxRange *= 0.75f;
        if (GrenadeDistance < maxRange)
        {
            SetSpotted();
        }
    }

    private readonly BotComponent _bot;
    private BotOwner BotOwner
    {
        get { return _bot.BotOwner; }
    }

    public float GrenadeDistance { get; private set; }

    public void Update()
    {
        if (BotOwner == null || BotOwner.IsDead || Grenade == null || _sentToBot)
        {
            return;
        }

        if (!_sentToBot && CanReact)
        {
            _sentToBot = true;
            var collisionSound = Grenade.GrenadeSettings.CollisionSound;
            bool isFrag = collisionSound == GrenadeSettings.CollisionSounds.frag;
            var trigger = isFrag ? EPhraseTrigger.OnEnemyGrenade : EPhraseTrigger.Look;
            _bot.Talk.GroupSay(trigger, ETagStatus.Combat, false, 100);

            return;
        }

        if (Spotted)
        {
            return;
        }

        GrenadeDistance = (Grenade.transform.position - BotOwner.Position).magnitude;

        // Something landing at your feet registers whether or not you were looking at it.
        if (GrenadeDistance < NOTICE_WITHOUT_LOOKING_DISTANCE)
        {
            SetSpotted();
            return;
        }

        if (_nextCheckRaycastTime < Time.time)
        {
            _nextCheckRaycastTime = Time.time + 0.05f;
            if (CheckVisibility())
            {
                SetSpotted();
            }
        }
    }

    private bool _sentToBot;

    private void SetSpotted()
    {
        if (!Spotted)
        {
            TimeSpotted = Time.time;
            Spotted = true;
        }
    }

    private bool CheckVisibility()
    {
        Vector3 lookPoint = _bot.Transform.WeaponRoot;
        Vector3 lookDir = _bot.LookDirection;

        Vector3 grenadePos = Grenade.transform.position + (Vector3.up * 0.05f);
        Vector3 grenadeDir = grenadePos - lookPoint;
        if (Vector3.Dot(lookDir, grenadeDir.normalized) < 0.25f)
        {
            return false; // Not looking in the right direction
        }

        return !Physics.Raycast(lookPoint, grenadeDir.normalized, grenadeDir.magnitude, LayersMaskController.HighPolyWithTerrainMaskAI);
    }

    public void UpdateGrenadeDanger(Vector3 Danger)
    {
        DangerPoint = Danger;
    }

    private float TimeSpotted { get; set; }
    public float TimeSinceSpotted
    {
        get { return Spotted ? Time.time - TimeSpotted : 0f; }
    }

    public Grenade Grenade { get; private set; }
    public Vector3 DangerPoint { get; set; }
    private bool Spotted { get; set; }
    public bool CanReact
    {
        get { return WillNotice && Spotted && TimeSinceSpotted > _reactionTime; }
    }

    private readonly float _reactionTime;
    private float _nextCheckRaycastTime;
}
