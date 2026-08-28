using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Helpers;

namespace SAINServerMod.Extensions;

public static class PresetTunerExtensions
{
    public static void ApplyTuning(
        this SAINDifficulty difficulty,
        GlobalSettingsClass global,
        Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings
    )
    {
        switch (difficulty)
        {
            case SAINDifficulty.easy:
                global.ApplyEasy(botSettings);
                break;
            case SAINDifficulty.lesshard:
                global.ApplyNormal(botSettings);
                break;
            case SAINDifficulty.hard:
                global.ApplyBase(botSettings);
                break;
            case SAINDifficulty.harderpmcs:
                global.ApplyBase(botSettings);
                botSettings.ApplyHarderPMCs();
                break;
            case SAINDifficulty.veryhard:
                global.ApplyVeryHard(botSettings);
                break;
            case SAINDifficulty.deathwish:
                global.ApplyImpossible(botSettings);
                break;
        }
    }

    private static void ApplyEasy(this GlobalSettingsClass global, Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        ApplyBase(global, botSettings);

        global.Shoot.BOT_RECOIL_COEF = 3f;
        global.Difficulty.ScatteringCoef = 2f;
        global.Difficulty.PRECISION_SPEED_COEF = 0.33f;
        global.Difficulty.ACCURACY_SPEED_COEF = 2f;
        global.Difficulty.HearingDistanceCoef = 0.4f;
        global.Aiming.FasterCQBReactionsGlobal = false;
        global.Difficulty.VisibleDistCoef = 0.5f;
        global.Difficulty.GainSightCoef = 0.33f;

        foreach (var bot in botSettings)
        {
            bot.Value.DifficultyModifier = MathExt.Round100(MathExt.Clamp(bot.Value.DifficultyModifier * 0.5f, 0.01f, 2f));
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 120f;
                setting.Value.Shoot.FireratMulti *= 0.4f;
                setting.Value.Shoot.BurstMulti *= 0.5f;
                if (setting.Value.Aiming.MAX_AIM_TIME < 2f)
                {
                    setting.Value.Aiming.MAX_AIM_TIME = 2f;
                }
                if (setting.Value.Aiming.MAX_AIMING_UPGRADE_BY_TIME < 0.4f)
                {
                    setting.Value.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.4f;
                }
            }
        }
        foreach (var botsetting in botSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.5f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.55f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.65f;
            }
            if (
                botsetting.Key.IsPMC()
                || botsetting.Key == ESainWildSpawnType.exUsec
                || botsetting.Key == ESainWildSpawnType.pmcBot
                || botsetting.Key == ESainWildSpawnType.arenaFighter
                || botsetting.Key == ESainWildSpawnType.arenaFighterEvent
            )
            {
                var pmcSettings = botsetting.Value.Settings;

                var easy = pmcSettings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = pmcSettings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.5f;

                var hard = pmcSettings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.55f;

                var impossible = pmcSettings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.75f;
            }
            if (botsetting.Key == ESainWildSpawnType.assault || botsetting.Key == ESainWildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.45f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.45f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.5f;
            }
        }
    }

    private static void ApplyNormal(this GlobalSettingsClass global, Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        ApplyBase(global, botSettings);

        global.Shoot.BOT_RECOIL_COEF = 1.6f;
        global.Difficulty.ScatteringCoef = 1f;
        global.Difficulty.PRECISION_SPEED_COEF = 0.75f;
        global.Difficulty.ACCURACY_SPEED_COEF = 1f;
        global.Difficulty.VisibleDistCoef = 0.75f;
        global.Difficulty.GainSightCoef = 0.75f;
        global.Difficulty.HearingDistanceCoef = 0.66f;
        global.Aiming.FasterCQBReactionsGlobal = false;

        foreach (var bot in botSettings)
        {
            bot.Value.DifficultyModifier = MathExt.Round100(MathExt.Clamp(bot.Value.DifficultyModifier * 0.85f, 0.01f, 2f));
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 150f;
                setting.Value.Shoot.FireratMulti *= 0.8f;
            }
        }

        foreach (var botsetting in botSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.5f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.65f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.8f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (
                botsetting.Key.IsPMC()
                || botsetting.Key == ESainWildSpawnType.exUsec
                || botsetting.Key == ESainWildSpawnType.pmcBot
                || botsetting.Key == ESainWildSpawnType.arenaFighter
                || botsetting.Key == ESainWildSpawnType.arenaFighterEvent
            )
            {
                var pmcSettings = botsetting.Value.Settings;

                var easy = pmcSettings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.4f;

                var normal = pmcSettings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.6f;

                var hard = pmcSettings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.7f;

                var impossible = pmcSettings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.9f;
            }
            if (botsetting.Key == ESainWildSpawnType.assault || botsetting.Key == ESainWildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.35f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.45f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.5f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.65f;

                easy.Move.LEAN_TOGGLE = false;
                normal.Move.LEAN_TOGGLE = false;
                hard.Move.LEAN_TOGGLE = false;
                impossible.Move.LEAN_TOGGLE = false;
            }
        }
    }

    private static readonly ESainWildSpawnType[] _flashResistantBosses =
    [
        ESainWildSpawnType.bossTagilla,
        ESainWildSpawnType.bossTagillaAgro,
        ESainWildSpawnType.bossKilla,
        ESainWildSpawnType.bossKillaAgro,
    ];

    private const float FLASH_RESISTANT_DISORIENTATION = 0.25f;
    private const float FLASH_RESISTANT_DURATION = 0.5f;

    private static void ApplyFlashResistantBosses(this Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        foreach (ESainWildSpawnType type in _flashResistantBosses)
        {
            if (!botSettings.TryGetValue(type, out var group))
            {
                continue;
            }

            foreach (var pair in group.Settings)
            {
                pair.Value.Look.FlashDisorientation = FLASH_RESISTANT_DISORIENTATION;
                pair.Value.Look.FlashDurationMulti = FLASH_RESISTANT_DURATION;
            }
        }
    }

    private static void ApplyBase(this GlobalSettingsClass global, Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        global.Difficulty.ScatteringCoef = 0.75f;
        global.Difficulty.ACCURACY_SPEED_COEF = 0.8f;

        botSettings.ApplyFlashResistantBosses();

        foreach (var botsetting in botSettings)
        {
            var settings = botsetting.Value.Settings;
            var easy = settings[ESainBotDifficulty.easy];
            var normal = settings[ESainBotDifficulty.normal];
            var hard = settings[ESainBotDifficulty.hard];
            var impossible = settings[ESainBotDifficulty.impossible];

            easy.Move.LEAN_TOGGLE = true;
            normal.Move.LEAN_TOGGLE = true;
            hard.Move.LEAN_TOGGLE = true;
            impossible.Move.LEAN_TOGGLE = true;

            easy.Move.STRAFE_SPEED = 0.55f;
            normal.Move.STRAFE_SPEED = 0.75f;
            hard.Move.STRAFE_SPEED = 0.8f;
            impossible.Move.STRAFE_SPEED = 1f;

            easy.Core.VisibleAngle = 120f;
            normal.Core.VisibleAngle = 150f;
            hard.Core.VisibleAngle = 170f;
            impossible.Core.VisibleAngle = 180f;

            easy.Core.VisibleDistance = 150f;
            normal.Core.VisibleDistance = 225f;
            hard.Core.VisibleDistance = 250f;
            impossible.Core.VisibleDistance = 275f;

            easy.Aiming.FasterCQBReactions = false;
            normal.Aiming.FasterCQBReactions = true;
            hard.Aiming.FasterCQBReactions = true;
            impossible.Aiming.FasterCQBReactions = true;

            easy.Aiming.AimForHead = false;
            normal.Aiming.AimForHead = false;
            hard.Aiming.AimForHead = false;
            impossible.Aiming.AimForHead = false;

            easy.Aiming.AimForHeadChance = 1;
            normal.Aiming.AimForHeadChance = 10f;
            hard.Aiming.AimForHeadChance = 33f;
            impossible.Aiming.AimForHeadChance = 50f;

            switch (botsetting.Key)
            {
                case ESainWildSpawnType.gifter:
                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;
                    easy.Aiming.AimForHead = true;
                    normal.Aiming.AimForHead = true;
                    hard.Aiming.AimForHead = true;
                    impossible.Aiming.AimForHead = true;
                    break;

                case ESainWildSpawnType.pmcBEAR:
                case ESainWildSpawnType.pmcUSEC:

                    easy.Move.LEAN_TOGGLE = true;
                    normal.Move.LEAN_TOGGLE = true;
                    hard.Move.LEAN_TOGGLE = true;
                    impossible.Move.LEAN_TOGGLE = true;

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Core.VisibleAngle = 150f;
                    normal.Core.VisibleAngle = 160f;
                    hard.Core.VisibleAngle = 170f;
                    impossible.Core.VisibleAngle = 180f;

                    easy.Core.VisibleDistance = 200f;
                    normal.Core.VisibleDistance = 225f;
                    hard.Core.VisibleDistance = 250f;
                    impossible.Core.VisibleDistance = 275f;

                    break;

                case ESainWildSpawnType.assault:
                case ESainWildSpawnType.assaultGroup:
                case ESainWildSpawnType.cursedAssault:
                case ESainWildSpawnType.test:
                case ESainWildSpawnType.crazyAssaultEvent:

                    easy.Move.STRAFE_SPEED = 0.5f;
                    normal.Move.STRAFE_SPEED = 0.55f;
                    hard.Move.STRAFE_SPEED = 0.6f;
                    impossible.Move.STRAFE_SPEED = 0.65f;

                    easy.Core.GainSightCoef = 1f;
                    normal.Core.GainSightCoef = 1f;
                    hard.Core.GainSightCoef = 1f;
                    impossible.Core.GainSightCoef = 1f;

                    easy.Core.VisibleAngle = 120f;
                    normal.Core.VisibleAngle = 135f;
                    hard.Core.VisibleAngle = 140f;
                    impossible.Core.VisibleAngle = 150f;

                    easy.Core.VisibleDistance = 100f;
                    normal.Core.VisibleDistance = 125f;
                    hard.Core.VisibleDistance = 150f;
                    impossible.Core.VisibleDistance = 200f;

                    easy.Difficulty.HearingDistanceCoef = 0.5f;
                    normal.Difficulty.HearingDistanceCoef = 0.65f;
                    hard.Difficulty.HearingDistanceCoef = 0.75f;
                    impossible.Difficulty.HearingDistanceCoef = 1f;

                    easy.Aiming.FasterCQBReactions = false;
                    normal.Aiming.FasterCQBReactions = false;
                    hard.Aiming.FasterCQBReactions = false;
                    impossible.Aiming.FasterCQBReactions = false;

                    easy.Move.LEAN_TOGGLE = false;
                    normal.Move.LEAN_TOGGLE = false;
                    hard.Move.LEAN_TOGGLE = false;
                    impossible.Move.LEAN_TOGGLE = false;

                    break;

                case ESainWildSpawnType.arenaFighter:
                case ESainWildSpawnType.arenaFighterEvent:
                case ESainWildSpawnType.pmcBot:
                case ESainWildSpawnType.exUsec:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    break;

                case ESainWildSpawnType.bossTest:
                case ESainWildSpawnType.bossBoar:
                case ESainWildSpawnType.bossBoarSniper:
                case ESainWildSpawnType.bossSanitar:
                case ESainWildSpawnType.bossGluhar:
                case ESainWildSpawnType.bossBully:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    break;

                case ESainWildSpawnType.bossKilla:
                case ESainWildSpawnType.bossKillaAgro:
                case ESainWildSpawnType.bossTagilla:
                case ESainWildSpawnType.bossTagillaAgro:
                case ESainWildSpawnType.bossKolontay:

                    easy.Shoot.FireratMulti = 2f;
                    normal.Shoot.FireratMulti = 2f;
                    hard.Shoot.FireratMulti = 2f;
                    impossible.Shoot.FireratMulti = 2f;

                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    break;

                case ESainWildSpawnType.bossKojaniy:
                case ESainWildSpawnType.bossPartisan:

                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    break;

                case ESainWildSpawnType.followerTest:
                case ESainWildSpawnType.followerBully:
                case ESainWildSpawnType.followerGluharSnipe:
                case ESainWildSpawnType.followerGluharScout:
                case ESainWildSpawnType.followerGluharSecurity:
                case ESainWildSpawnType.followerGluharAssault:
                case ESainWildSpawnType.followerSanitar:
                case ESainWildSpawnType.followerTagilla:
                case ESainWildSpawnType.tagillaHelperAgro:
                case ESainWildSpawnType.followerKojaniy:
                case ESainWildSpawnType.followerBoar:
                case ESainWildSpawnType.followerBoarClose1:
                case ESainWildSpawnType.followerBoarClose2:
                case ESainWildSpawnType.followerKolontayAssault:
                case ESainWildSpawnType.followerKolontaySecurity:

                    easy.Move.STRAFE_SPEED = 0.55f;
                    normal.Move.STRAFE_SPEED = 0.75f;
                    hard.Move.STRAFE_SPEED = 0.8f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    break;

                case ESainWildSpawnType.sectantWarrior:
                case ESainWildSpawnType.sectantPriest:
                case ESainWildSpawnType.sectactPriestEvent:
                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    break;

                case ESainWildSpawnType.bossKnight:
                case ESainWildSpawnType.followerBigPipe:
                case ESainWildSpawnType.followerBirdEye:

                    easy.Move.STRAFE_SPEED = 1f;
                    normal.Move.STRAFE_SPEED = 1f;
                    hard.Move.STRAFE_SPEED = 1f;
                    impossible.Move.STRAFE_SPEED = 1f;

                    easy.Aiming.AimForHead = true;
                    normal.Aiming.AimForHead = true;
                    hard.Aiming.AimForHead = true;
                    impossible.Aiming.AimForHead = true;

                    easy.Aiming.FasterCQBReactions = true;
                    normal.Aiming.FasterCQBReactions = true;
                    hard.Aiming.FasterCQBReactions = true;
                    impossible.Aiming.FasterCQBReactions = true;

                    easy.Difficulty.ScatteringCoef = 0.5f;
                    normal.Difficulty.ScatteringCoef = 0.5f;
                    hard.Difficulty.ScatteringCoef = 0.5f;
                    impossible.Difficulty.ScatteringCoef = 0.5f;

                    easy.Difficulty.AggressionCoef = 2f;
                    normal.Difficulty.AggressionCoef = 2f;
                    hard.Difficulty.AggressionCoef = 2f;
                    impossible.Difficulty.AggressionCoef = 2f;

                    easy.Aiming.AimForHeadChance = 15;
                    normal.Aiming.AimForHeadChance = 25f;
                    hard.Aiming.AimForHeadChance = 35f;
                    impossible.Aiming.AimForHeadChance = 50f;
                    break;

                case ESainWildSpawnType.marksman:
                case ESainWildSpawnType.bossZryachiy:
                case ESainWildSpawnType.followerZryachiy:
                case ESainWildSpawnType.peacefullZryachiyEvent:
                case ESainWildSpawnType.ravangeZryachiyEvent:
                case ESainWildSpawnType.shooterBTR:
                case ESainWildSpawnType.spiritWinter:
                case ESainWildSpawnType.spiritSpring:
                case ESainWildSpawnType.peacemaker:
                case ESainWildSpawnType.skier:
                case ESainWildSpawnType.sectantPredvestnik:
                case ESainWildSpawnType.sectantPrizrak:
                case ESainWildSpawnType.sectantOni:
                case ESainWildSpawnType.infectedAssault:
                case ESainWildSpawnType.infectedPmc:
                case ESainWildSpawnType.infectedCivil:
                case ESainWildSpawnType.infectedLaborant:
                case ESainWildSpawnType.infectedTagilla:
                    break;

                default:
                    break;
            }
        }
    }

    private static void ApplyHarderPMCs(this Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        foreach (var botsetting in botSettings)
        {
            if (botsetting.Key == ESainWildSpawnType.pmcUSEC || botsetting.Key == ESainWildSpawnType.pmcBEAR)
            {
                var pmcSettings = botsetting.Value.Settings;

                // Set for all difficulties
                foreach (var diff in pmcSettings.Values)
                {
                    //diff.Core.ScatteringPerMeter = 0.03f;
                    //diff.Core.ScatteringClosePerMeter = 0.080f;
                    diff.Mind.WeaponProficiency = 0.75f;
                    diff.Difficulty.ScatteringCoef = 0.6f;
                    diff.Difficulty.PRECISION_SPEED_COEF = 1.33f;
                    diff.Difficulty.ACCURACY_SPEED_COEF = 0.6f;
                    diff.Difficulty.GainSightCoef = 1.25f;
                    diff.Difficulty.VisibleDistCoef = 1.25f;
                    diff.Difficulty.AggressionCoef = 1.2f;
                }

                var easy = pmcSettings[ESainBotDifficulty.easy];
                easy.Aiming.FasterCQBReactionsDistance = 20f;
                easy.Aiming.FasterCQBReactionsMinimum = 0.3f;
                easy.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.35f;
                easy.Aiming.MAX_AIM_TIME = 1.5f;
                easy.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.65f;
                easy.Core.VisibleDistance = 200f;

                var normal = pmcSettings[ESainBotDifficulty.normal];
                normal.Aiming.FasterCQBReactionsDistance = 35f;
                normal.Aiming.FasterCQBReactionsMinimum = 0.25f;
                normal.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.4f;
                normal.Aiming.MAX_AIM_TIME = 1.35f;
                normal.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;
                normal.Core.VisibleDistance = 225f;

                var hard = pmcSettings[ESainBotDifficulty.hard];
                hard.Aiming.FasterCQBReactionsDistance = 50f;
                hard.Aiming.FasterCQBReactionsMinimum = 0.2f;
                hard.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.2f;
                hard.Aiming.MAX_AIM_TIME = 1.15f;
                hard.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.35f;
                hard.Core.VisibleDistance = 250f;

                var impossible = pmcSettings[ESainBotDifficulty.impossible];
                impossible.Aiming.FasterCQBReactionsDistance = 60f;
                impossible.Aiming.FasterCQBReactionsMinimum = 0.15f;
                impossible.Aiming.MAX_AIMING_UPGRADE_BY_TIME = 0.15f;
                impossible.Aiming.MAX_AIM_TIME = 1.0f;
                impossible.Aiming.BASE_HIT_AFFECTION_DELAY_SEC = 0.25f;
                impossible.Core.VisibleDistance = 275f;
            }
        }
    }

    private static void ApplyVeryHard(this GlobalSettingsClass global, Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        ApplyBase(global, botSettings);

        global.Shoot.BOT_RECOIL_COEF = 0.75f;
        global.Difficulty.ScatteringCoef = 0.55f;
        global.Difficulty.VisibleDistCoef = 1.25f;
        global.Difficulty.GainSightCoef = 1.25f;
        global.Difficulty.PRECISION_SPEED_COEF = 1.25f;
        global.Difficulty.ACCURACY_SPEED_COEF = 0.6f;

        botSettings.ApplyHarderPMCs();

        foreach (var bot in botSettings)
        {
            bot.Value.DifficultyModifier = MathExt.Round100(MathExt.Clamp(bot.Value.DifficultyModifier * 1.33f, 0.01f, 2f));
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 170f;
                setting.Value.Shoot.FireratMulti = 1.5f;
                setting.Value.Shoot.BurstMulti = 2f;
            }
        }
        foreach (var botsetting in botSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.85f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (
                botsetting.Key.IsPMC()
                || botsetting.Key == ESainWildSpawnType.exUsec
                || botsetting.Key == ESainWildSpawnType.pmcBot
                || botsetting.Key == ESainWildSpawnType.arenaFighter
                || botsetting.Key == ESainWildSpawnType.arenaFighterEvent
            )
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.85f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key == ESainWildSpawnType.assault || botsetting.Key == ESainWildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.65f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.7f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.75f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 0.9f;
            }
        }
    }

    private static void ApplyImpossible(this GlobalSettingsClass global, Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> botSettings)
    {
        ApplyBase(global, botSettings);

        global.Shoot.BOT_RECOIL_COEF = 0.5f;

        global.Difficulty.ScatteringCoef = 0.01f;
        global.Difficulty.VisibleDistCoef = 2.5f;
        global.Difficulty.GainSightCoef = 2f;
        global.Difficulty.PRECISION_SPEED_COEF = 3f;
        global.Difficulty.ACCURACY_SPEED_COEF = 0.1f;

        global.Look.NotLooking.NotLookingToggle = false;

        botSettings.ApplyHarderPMCs();

        foreach (var bot in botSettings)
        {
            foreach (var setting in bot.Value.Settings)
            {
                setting.Value.Core.VisibleAngle = 180f;
                setting.Value.Shoot.FireratMulti = 3f;
                setting.Value.Shoot.BurstMulti = 3f;
                setting.Value.Core.VisibleAngle = 180;
                setting.Value.Aiming.AimForHead = true;
                setting.Value.Aiming.AimForHeadChance = 66f;
            }
        }
        foreach (var botsetting in botSettings)
        {
            if (botsetting.Key.IsBossOrFollower())
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.85f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.9f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 1f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (
                botsetting.Key.IsPMC()
                || botsetting.Key == ESainWildSpawnType.exUsec
                || botsetting.Key == ESainWildSpawnType.pmcBot
                || botsetting.Key == ESainWildSpawnType.arenaFighter
                || botsetting.Key == ESainWildSpawnType.arenaFighterEvent
            )
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.75f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.9f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 1.0f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
            if (botsetting.Key == ESainWildSpawnType.assault || botsetting.Key == ESainWildSpawnType.assaultGroup)
            {
                var settings = botsetting.Value.Settings;

                var easy = settings[ESainBotDifficulty.easy];
                easy.Move.STRAFE_SPEED = 0.65f;

                var normal = settings[ESainBotDifficulty.normal];
                normal.Move.STRAFE_SPEED = 0.75f;

                var hard = settings[ESainBotDifficulty.hard];
                hard.Move.STRAFE_SPEED = 0.9f;

                var impossible = settings[ESainBotDifficulty.impossible];
                impossible.Move.STRAFE_SPEED = 1.0f;
            }
        }
    }
}
