using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using SAIN.Preset.Server;
using SAIN.Preset.Shared;

namespace SAIN.Helpers;

internal class HelpersGClass
{
    public static float LAY_DOWN_ANG_SHOOT
    {
        get { return BotInternalSettingsController.Core.LAY_DOWN_ANG_SHOOT; }
    }

    public static float Gravity
    {
        get { return BotInternalSettingsController.Core.G; }
    }

    public static float SMOKE_GRENADE_RADIUS_COEF
    {
        get { return BotInternalSettingsController.Core.SMOKE_GRENADE_RADIUS_COEF; }
    }
}

public class TemporaryStatModifiers
{
    public TemporaryStatModifiers(
        float precision = 1f,
        float accuracySpeed = 1f,
        float gainSight = 1f,
        float scatter = 1f,
        float priorityScatter = 1f,
        float visibleDistance = 1f,
        float hearingDistance = 1f
    )
    {
        Modifiers = new BotSettingsInGameModif
        {
            PrecicingSpeedCoef = precision,
            AccuratySpeedCoef = accuracySpeed,
            RuntimeVisionEffectK = gainSight,
            ScatteringCoef = scatter,
            PriorityScatteringCoef = priorityScatter,
            VisibleDistCoef = visibleDistance,
            HearingDistCoef = hearingDistance,
        };
    }

    public BotSettingsInGameModif Modifiers;
}

public class EFTCoreSettings
{
    private static CoreOverrides _overrides;

    public static bool Load()
    {
        try
        {
            string json = ServerDataClient.Get(nameof(CoreOverrides));

            if (string.IsNullOrEmpty(json))
            {
                Logger.LogError("[SAIN] Server returned no core overrides.");
                return false;
            }
            _overrides = JsonConvert.DeserializeObject<CoreOverrides>(json);

            if (_overrides == null)
            {
                Logger.LogError("[SAIN] Could not deserialize the core overrides sent by the server.");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to load core overrides from server: {ex.Message}");
            throw;
        }
    }

    public static void UpdateCoreSettings()
    {
        try
        {
            var core = BotInternalSettingsController.Core;

            if (_overrides == null)
            {
                Logger.LogError("[SAIN] Core overrides were never loaded from the server; not applying.");
                return;
            }

            core.SCAV_GROUPS_TOGETHER = _overrides.SCAV_GROUPS_TOGETHER;
            core.DIST_NOT_TO_GROUP = _overrides.DIST_NOT_TO_GROUP;
            core.DIST_NOT_TO_GROUP_SQR = core.DIST_NOT_TO_GROUP.Sqr();
            core.CAN_SHOOT_TO_HEAD = _overrides.CAN_SHOOT_TO_HEAD;
            core.SOUND_DOOR_OPEN_METERS = _overrides.SOUND_DOOR_OPEN_METERS;
            core.SOUND_DOOR_BREACH_METERS = _overrides.SOUND_DOOR_BREACH_METERS;
            core.JUMP_SPREAD_DIST = _overrides.JUMP_SPREAD_DIST;
            core.BASE_WALK_SPEREAD2 = _overrides.BASE_WALK_SPEREAD2;
            core.GRENADE_PRECISION = _overrides.GRENADE_PRECISION;
            core.PRONE_POSE = _overrides.PRONE_POSE;
            core.MOVE_COEF = _overrides.MOVE_COEF;
            core.LOWER_POSE = _overrides.LOWER_POSE;
            core.MAX_POSE = _overrides.MAX_POSE;
            core.FLARE_POWER = _overrides.FLARE_POWER;
            core.FLARE_TIME = _overrides.FLARE_TIME;
            core.SHOOT_TO_CHANGE_RND_PART_DELTA = _overrides.SHOOT_TO_CHANGE_RND_PART_DELTA;

            ModDetection.UpdateArmorClassCoef();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    public static void UpdateArmorClassCoef(float coef)
    {
        BotInternalSettingsController.Core.ARMOR_CLASS_COEF = coef;
    }
}

public class EFTBotSettings
{
    [JsonConstructor]
    public EFTBotSettings() { }

    public EFTBotSettings(string name, WildSpawnType type, BotDifficulty[] difficulties)
    {
        Name = name;
        WildSpawnType = type;
        foreach (BotDifficulty diff in difficulties)
        {
            Settings.Add(diff, BotInternalSettingsController.GetSettings(diff, type, true));
        }
    }

    [JsonProperty]
    public string Name;

    [JsonProperty]
    public WildSpawnType WildSpawnType;

    [JsonProperty]
    public Dictionary<BotDifficulty, BotSettingsComponents> Settings = new();
}
