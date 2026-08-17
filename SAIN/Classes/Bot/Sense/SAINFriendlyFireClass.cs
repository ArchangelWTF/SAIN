using EFT;
using SAIN.Components;
using SAIN.Preset.Shared.Enums;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINFriendlyFireClass : BotComponentClassBase
{
    public bool ClearShot
    {
        get { return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock; }
    }

    public FriendlyFireStatus FriendlyFireStatus { get; private set; }

    public SAINFriendlyFireClass(BotComponent sain)
        : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotInCombat;
    }

    public override void ManualUpdate()
    {
        if (FriendlyFireStatus == FriendlyFireStatus.FriendlyBlock)
        {
            BotOwner.ShootData?.EndShoot();
        }
        base.ManualUpdate();
    }

    public bool UpdateFriendlyFireStatus(Vector3 target, Vector3 weaponFirePort, Vector3 weaponPointDirection, BotComponent bot)
    {
        FriendlyFireStatus = CheckFriendlyFireStatus(target, weaponFirePort, weaponPointDirection, bot);
        return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
    }

    public bool UpdateFriendlyFireStatus(float distance, Vector3 weaponFirePort, Vector3 weaponPointDirection, BotComponent bot)
    {
        FriendlyFireStatus = CheckFriendlyFireStatus(distance, weaponFirePort, weaponPointDirection, bot);
        return FriendlyFireStatus != FriendlyFireStatus.FriendlyBlock;
    }

    public static FriendlyFireStatus CheckFriendlyFireStatus(
        float distance,
        Vector3 weaponFirePort,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        var members = bot.Squad?.Members;
        if (members == null || members.Count <= 1)
        {
            return FriendlyFireStatus.None;
        }
        return CheckFriendlyFire(weaponFirePort, distance, weaponPointDirection, bot);
    }

    public static FriendlyFireStatus CheckFriendlyFireStatus(
        Vector3 target,
        Vector3 weaponFirePort,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        var members = bot.Squad?.Members;
        if (members == null || members.Count <= 1)
        {
            return FriendlyFireStatus.None;
        }
        return CheckFriendlyFire(weaponFirePort, (weaponFirePort - target).magnitude, weaponPointDirection, bot);
    }

    public static FriendlyFireStatus CheckFriendlyFire(
        Vector3 weaponFirePort,
        float distance,
        Vector3 weaponPointDirection,
        BotComponent bot
    )
    {
        int count = SphereCastNonAlloc(weaponFirePort, distance, weaponPointDirection);
        if (count == 0)
        {
            return FriendlyFireStatus.None;
        }
        RaycastHit[] hits = _sphereCastHits;

        for (int i = 0; i < count; i++)
        {
            var hit = hits[i];
            if (hit.collider == null)
            {
                continue;
            }

            Player player = GameWorldComponent.Instance.GameWorld.GetPlayerByCollider(hit.collider);
            if (player == null)
            {
                continue;
            }

            if (player.ProfileId == bot.ProfileId)
            {
                continue;
            }

            if (!bot.EnemyController.IsPlayerAnEnemy(player.ProfileId))
            {
                return FriendlyFireStatus.FriendlyBlock;
            }
        }
        return FriendlyFireStatus.Clear;
    }

    private static readonly RaycastHit[] _sphereCastHits = new RaycastHit[32];

    private static int SphereCastNonAlloc(Vector3 weaponFirePort, float targetDistance, Vector3 weaponPointDirection)
    {
        const float sphereCastRadius = 0.2f;
        return Physics.SphereCastNonAlloc(
            weaponFirePort,
            sphereCastRadius,
            weaponPointDirection,
            _sphereCastHits,
            targetDistance,
            LayersMaskController.PlayerMask
        );
    }
}
