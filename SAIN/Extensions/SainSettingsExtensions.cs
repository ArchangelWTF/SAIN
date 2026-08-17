using EFT;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

namespace SAIN.Extensions;

internal static class SainSettingsExtensions
{
    public static void Apply(this SAINAimingSettings s, BotSettingsComponents settings)
    {
        settings.Aiming.MAX_AIMING_UPGRADE_BY_TIME = s.MAX_AIMING_UPGRADE_BY_TIME;
        settings.Aiming.DIST_TO_SHOOT_NO_OFFSET = s.DIST_TO_SHOOT_NO_OFFSET;
        settings.Aiming.COEF_IF_MOVE = s.COEF_IF_MOVE;
        settings.Aiming.TIME_COEF_IF_MOVE = s.TIME_COEF_IF_MOVE;
        settings.Aiming.MAX_AIM_TIME = s.MAX_AIM_TIME;
        settings.Aiming.AIMING_TYPE = s.AIMING_TYPE;
        settings.Aiming.DAMAGE_TO_DISCARD_AIM_0_100 = s.DAMAGE_TO_DISCARD_AIM_0_100;
        settings.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = s.BASE_HIT_AFFECTION_DELAY_SEC;
        settings.Aiming.MIN_TIME_DISCARD_AIM_SEC = s.MIN_TIME_DISCARD_AIM_SEC;
        settings.Aiming.MAX_TIME_DISCARD_AIM_SEC = s.MAX_TIME_DISCARD_AIM_SEC;
        settings.Aiming.ANY_PART_SHOOT_TIME = s.ANY_PART_SHOOT_TIME;
        settings.Aiming.FIRST_CONTACT_ADD_SEC = s.FIRST_CONTACT_ADD_SEC;
        settings.Aiming.FIRST_CONTACT_ADD_CHANCE_100 = s.FIRST_CONTACT_ADD_CHANCE_100;
        settings.Aiming.OFFSET_RECAL_ANYWAY_TIME = s.OFFSET_RECAL_ANYWAY_TIME;
    }

    public static void Apply(this SAINBossSettings s, BotSettingsComponents settings)
    {
        settings.Boss.SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = s.SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY;
    }

    public static void Apply(this SAINCoreSettings s, BotSettingsComponents settings)
    {
        settings.Core.VisibleAngle = s.VisibleAngle;
        settings.Core.VisibleDistance = s.VisibleDistance;
        settings.Look.BASE_RUNTIME_EFFECT_K = s.GainSightCoef;
        settings.Core.AccuratySpeed = s.AccuratySpeed;
        settings.Core.ScatteringPerMeter = s.ScatteringPerMeter;
        settings.Core.ScatteringClosePerMeter = s.ScatteringClosePerMeter;
        settings.Core.HearingSense = s.HearingDistanceMulti;
        settings.Core.CanGrenade = s.CanGrenade;
        settings.Core.CanRun = s.CanRun;
        settings.Core.DamageCoeff = s.DamageCoeff;
    }

    public static void Apply(this SAINGrenadeSettings s, BotSettingsComponents settings)
    {
        settings.Grenade.GrenadePrecision = s.GrenadePrecision;
        settings.Grenade.CAN_THROW_STRAIGHT_CONTACT = s.CAN_THROW_STRAIGHT_CONTACT;
        settings.Grenade.DELTA_NEXT_ATTEMPT = s.ThrowGrenadeFrequency;
        settings.Grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;
        settings.Grenade.MIN_THROW_DIST_PERCENT_0_1 = s.MIN_THROW_DIST_PERCENT_0_1;
        settings.Grenade.MIN_DIST_NOT_TO_THROW = s.MinEnemyDistance;
        settings.Grenade.DELTA_GRENADE_START_TIME = s.DELTA_GRENADE_START_TIME;
        settings.Grenade.BEWARE_TYPE = s.BEWARE_TYPE;
    }

    public static void Apply(this SAINLookSettings s, BotSettingsComponents settings)
    {
        settings.Look.CAN_USE_LIGHT = s.CAN_USE_LIGHT;
        settings.Look.FULL_SECTOR_VIEW = s.FULL_SECTOR_VIEW;
        settings.Look.MAX_DIST_CLAMP_TO_SEEN_SPEED = s.MAX_DIST_CLAMP_TO_SEEN_SPEED;
        settings.Look.VISIBLE_ANG_NIGHTVISION = s.VISIBLE_ANG_NIGHTVISION;
        settings.Look.VISIBLE_ANG_LIGHT = s.VISIBLE_ANG_LIGHT;
        settings.Look.VISIBLE_DISNACE_WITH_LIGHT = s.VISIBLE_DISNACE_WITH_LIGHT;
        settings.Look.GOAL_TO_FULL_DISSAPEAR = s.GOAL_TO_FULL_DISSAPEAR;
        settings.Look.GOAL_TO_FULL_DISSAPEAR_GREEN = s.GOAL_TO_FULL_DISSAPEAR_GREEN;
        settings.Look.GOAL_TO_FULL_DISSAPEAR_SHOOT = s.GOAL_TO_FULL_DISSAPEAR_SHOOT;
        settings.Look.SHOOT_FROM_EYES = s.SHOOT_FROM_EYES;
        settings.Look.COEF_REPEATED_SEEN = s.COEF_REPEATED_SEEN;
    }

    public static void Apply(this SAINMindSettings s, BotSettingsComponents settings)
    {
        settings.Mind.UNDER_FIRE_PERIOD = 5f;
        settings.Mind.CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;
        settings.Mind.FOOD_DRINK_DELAY_SEC = 240f;
        settings.Mind.CAN_USE_MEDS = true;
        settings.Mind.CAN_USE_FOOD_DRINK = true;
        settings.Mind.HIT_DELAY_WHEN_PEACE = 0.4f;
        settings.Mind.HIT_DELAY_WHEN_HAVE_SMT = 0.1f;
    }

    public static void Apply(this SAINScatterSettings s, BotSettingsComponents settings)
    {
        settings.Scattering.HandDamageScatteringMinMax = s.HandDamageScatteringMinMax;
        settings.Scattering.HandDamageAccuracySpeed = s.HandDamageAccuracySpeed;
        settings.Scattering.DIST_NOT_TO_SHOOT = s.DIST_NOT_TO_SHOOT;
        settings.Scattering.FromShot = s.FromShot;
    }

    public static void Apply(this SAINShootSettings s, BotSettingsComponents settings)
    {
        settings.Shoot.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 0f;
        settings.Shoot.BASE_AUTOMATIC_TIME = 0.5f;
        settings.Shoot.CAN_STOP_SHOOT_CAUSE_ANIMATOR = false;
        settings.Shoot.RECOIL_DELTA_PRESS = float.MaxValue;
    }

    public static void SetConfigValues(this SAINSettingsClass sainFileSettings, BotOwner botOwner)
    {
        var eftFileSettings = botOwner.Settings.FileSettings;
        sainFileSettings.Aiming.Apply(eftFileSettings);
        sainFileSettings.Boss.Apply(eftFileSettings);
        sainFileSettings.Grenade.Apply(eftFileSettings);
        sainFileSettings.Look.Apply(eftFileSettings);
        sainFileSettings.Mind.Apply(eftFileSettings);
        sainFileSettings.Scattering.Apply(eftFileSettings);
        sainFileSettings.Shoot.Apply(eftFileSettings);
    }
}
