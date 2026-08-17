using EFT;

namespace SAIN.Extensions;

public static class BotExtensions
{
    public static bool IsBotActive(this BotOwner botOwner)
    {
        if (botOwner == null || botOwner.StandBy == null)
        {
            return false;
        }

        var standByType = botOwner.StandBy.StandByType;

        if (standByType == BotStandByType.none || standByType == BotStandByType.active)
        {
            return true;
        }

        return false;
    }

    public static bool IsBotNotNullOrDead(this BotOwner botOwner)
    {
        return botOwner != null
            && botOwner.gameObject != null
            && botOwner.gameObject.transform != null
            && botOwner.Transform != null
            && !botOwner.IsDead;
    }
}
