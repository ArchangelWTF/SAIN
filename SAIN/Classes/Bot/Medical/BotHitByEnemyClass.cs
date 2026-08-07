using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using EFT.Ballistics;

namespace SAIN.SAINComponent.Classes;

public class BotHitByEnemyClass : BotBase
{
    public Enemy EnemyWhoLastShotMe { get; private set; }

    public BotHitByEnemyClass(BotComponent bot)
        : base(bot)
    {
        CanEverTick = false;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += clearEnemy;
        base.Init();
    }

    public void GetHit(DamageInfo DamageInfo, EBodyPart bodyPart, float floatVal)
    {
        var player = DamageInfo.Player?.iPlayer;
        if (player == null)
        {
            return;
        }
        Enemy enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
        if (enemy == null)
        {
            return;
        }
        EnemyWhoLastShotMe = enemy;
        enemy.Status.GetHit(DamageInfo);
    }

    private void clearEnemy(string profileId, Enemy enemy)
    {
        if (enemy == EnemyWhoLastShotMe)
        {
            EnemyWhoLastShotMe = null;
        }
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= clearEnemy;
        base.Dispose();
    }
}
