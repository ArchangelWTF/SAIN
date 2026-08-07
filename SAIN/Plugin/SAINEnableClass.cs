using System.Collections.Generic;
using EFT;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Preset.GlobalSettings;
using static SAIN.Helpers.EnumValues;

namespace SAIN;

public static class SAINEnableClass
{
    static SAINEnableClass()
    {
        GameWorld.OnDispose += Clear;
    }

    private static readonly HashSet<string> ExcludedBots = [];
    private static readonly HashSet<string> EnabledBots = [];

    /// <summary>
    /// Checks if this bot has SAIN enabled or if it is a vanilla bot.
    /// </summary>
    public static bool IsSAINDisabledForBot(BotOwner botOwner)
    {
        if (botOwner == null)
        {
            return true;
        }

        Player player = botOwner.GetPlayer;
        if (player == null)
        {
            return true;
        }

        string id = player.ProfileId;
        if (ExcludedBots.Contains(id))
        {
            return true;
        }

        if (EnabledBots.Contains(id))
        {
            return false;
        }

        ProfileSettings settings = botOwner.Profile?.Info?.Settings;
        if (settings == null)
        {
            return true;
        }

        player.OnIPlayerDeadOrUnspawn += ClearBot;

        if (IsBotExcluded(botOwner))
        {
            ExcludedBots.Add(id);
#if DEBUG
            Logger.LogDebug($"Added Excluded Bot [{player.Profile.Nickname},{id}]");
#endif
            return true;
        }
        EnabledBots.Add(id);
#if DEBUG
        Logger.LogDebug($"Added Enabled Bot [{player.Profile.Nickname},{id}]");
#endif
        return false;
    }

    /// <summary>
    /// Checks if this IPlayer has SAIN enabled or if it is a vanilla bot.
    /// </summary>
    public static bool IsSAINDisabledForBot(IPlayer iPlayer)
    {
        if (iPlayer == null || !iPlayer.IsAI)
        {
            return true;
        }

        BotOwner botOwner = iPlayer.AIData?.BotOwner;
        if (botOwner == null)
        {
            return true;
        }

        string id = iPlayer.ProfileId;
        if (ExcludedBots.Contains(id))
        {
            return true;
        }

        if (EnabledBots.Contains(id))
        {
            return false;
        }

        ProfileSettings settings = iPlayer.Profile?.Info?.Settings;
        if (settings == null)
        {
            return true;
        }

        botOwner.GetPlayer.OnIPlayerDeadOrUnspawn += ClearBot;

        if (IsBotExcluded(botOwner))
        {
            ExcludedBots.Add(id);
            return true;
        }
        EnabledBots.Add(id);
        return false;
    }

    private static void Clear()
    {
        ExcludedBots.Clear();
        EnabledBots.Clear();
    }

    private static void ClearBot(IPlayer player)
    {
        if (player != null)
        {
            player.OnIPlayerDeadOrUnspawn -= ClearBot;
            string id = player.ProfileId;
            ExcludedBots.Remove(id);
            EnabledBots.Remove(id);
        }
    }

    /// <summary>
    /// Checks if this bot has SAIN enabled or if it is a vanilla bot.
    /// </summary>
    public static bool IsBotExcluded(string profileId)
    {
        return !EnabledBots.Contains(profileId);
    }

    /// <summary>
    /// Checks if this bot has SAIN enabled or if it is a vanilla bot.
    /// </summary>
    public static bool IsBotExcluded(BotOwner botOwner)
    {
        var settings = botOwner.Profile?.Info?.Settings;
        if (settings == null)
        {
            return true;
        }

        WildSpawnType type = settings.Role;

        if (BotSpawnController.StrictExclusionList.Contains(type))
        {
            return true;
        }

        if (IsAlwaysEnabled(type, botOwner))
        {
            return false;
        }

        return ShallExludeByWildSpawnType(type, botOwner);
    }

    public static bool ShallExludeByWildSpawnType(WildSpawnType wildSpawnType, BotOwner botOwner)
    {
        return ExcludeScav(wildSpawnType, botOwner)
            || ExcludeNormalBoss(wildSpawnType)
            || ExcludeNormalFollower(wildSpawnType)
            || ExcludeGoons(wildSpawnType)
            || ExcludeCultists(wildSpawnType)
            || ExcludeRogues(wildSpawnType)
            || ExcludeRaiders(wildSpawnType)
            || ExcludeBloodHounds(wildSpawnType)
            || ExcludeLabyrinthBots(wildSpawnType)
            || ExcludeSpecialBots(wildSpawnType);
    }

    private static bool IsAlwaysEnabled(WildSpawnType wildSpawnType, BotOwner botOwner)
    {
        return wildSpawnType.IsPmcBot() || BotManagerComponent.Instance?.Bots?.ContainsKey(botOwner.ProfileId) == true;
    }

    private static bool ExcludeScav(WildSpawnType wildSpawnType, BotOwner botOwner)
    {
        return SAINEnabled.VanillaScavs && WildSpawn.IsScav(wildSpawnType) && !IsPlayerScav(botOwner.Profile);
    }

    private static bool ExcludeNormalBoss(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaBosses && WildSpawn.IsNormalBoss(wildSpawnType);
    }

    private static bool ExcludeNormalFollower(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaFollowers && WildSpawn.IsNormalFollower(wildSpawnType);
    }

    private static bool ExcludeGoons(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaGoons && WildSpawn.IsGoons(wildSpawnType);
    }

    private static bool ExcludeCultists(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaCultists && wildSpawnType.IsSectant();
    }

    private static bool ExcludeRogues(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaRogues && wildSpawnType == WildSpawnType.exUsec;
    }

    private static bool ExcludeRaiders(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaRaiders && wildSpawnType == WildSpawnType.pmcBot;
    }

    private static bool ExcludeBloodHounds(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaBloodHounds && (wildSpawnType == WildSpawnType.arenaFighter || wildSpawnType == WildSpawnType.arenaFighterEvent);
    }

    private static bool ExcludeLabyrinthBots(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaLabyrinthBots && WildSpawn.IsLabyrinthBot(wildSpawnType);
    }

    private static bool ExcludeSpecialBots(WildSpawnType wildSpawnType)
    {
        return SAINEnabled.VanillaSpecialBots && WildSpawn.IsSpecialBot(wildSpawnType);
    }

    public static bool IsPlayerScav(Profile profile)
    {
        // Handle the old version of creating player Scavs
        if (profile.Info.Nickname.Contains(" ("))
        {
            return true;
        }
        // Check for player Scavs created by SPT
        return profile.Info.Settings.Role == WildSpawnType.assault && !string.IsNullOrEmpty(profile.Info.MainProfileNickname);
    }

    /// <summary>
    /// Is this player a sain bot, and are they also in combat state?
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static bool IsBotInCombat(IPlayer player)
    {
        GetSAIN(player.ProfileId, out var bot);
        return bot != null && bot.SAINLayersActive;
    }

    public static bool GetSAIN(string profileId, out BotComponent sain)
    {
        sain = null;
        if (profileId.IsNullOrEmpty())
        {
            return false;
        }

        if (!EnabledBots.Contains(profileId))
        {
            return false;
        }

        return BotManagerComponent.Instance != null
            && BotManagerComponent.Instance.BotSpawnController.BotDictionary.TryGetValue(profileId, out sain);
    }

    private static VanillaBotSettings SAINEnabled
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.General.VanillaBots; }
    }
}
