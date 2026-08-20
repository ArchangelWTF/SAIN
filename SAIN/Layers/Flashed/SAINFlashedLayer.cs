using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Extensions;
using SAIN.Models.Enums;

namespace SAIN.Layers.Flashed;

internal class SAINFlashedLayer(BotOwner bot, int priority) : SAINLayer(bot, priority, Name, ESAINLayer.Flashed)
{
    public static readonly string Name = BuildLayerName("Flashed");

    public override Action GetNextAction()
    {
        return new Action(typeof(FlashedAction), "Flashed");
    }

    public override bool IsActive()
    {
        if (!BotOwner.IsBotActive())
        {
            CheckActiveChanged(false);
            return false;
        }

        bool active = GetBotComponent() && Bot.Flashed.IsFlashed;
        CheckActiveChanged(active);
        return active;
    }

    public override bool IsCurrentActionEnding()
    {
        return false;
    }
}
