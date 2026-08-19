using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.Shared.GlobalSettings.Categories.Look;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Sense;

public class BotFlashedClass : BotComponentClassBase
{
    private const float REMEMBERED_HEIGHT = 1.2f;

    private const float SEARCH_POINT_RANGE = 140f;

    public BotFlashedClass(BotComponent bot)
        : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public bool IsFlashed
    {
        get { return _flashEndTime > Time.time; }
    }

    public float TimeRemaining
    {
        get { return Mathf.Max(0f, _flashEndTime - Time.time); }
    }

    public Vector3? LastSeenEnemyPoint { get; private set; }

    public bool BlindFireReady
    {
        get { return _blindFireTime < Time.time; }
    }

    public void ApplyFlash(float baseTime, Vector3 position)
    {
        var settings = Settings;

        float duration = baseTime * settings.DurationMultiplier * Bot.Info.FileSettings.Look.FlashDurationMulti;
        if (BotOwner.NightVision.UsingNow)
        {
            duration *= settings.NightVisionMultiplier;
        }
        duration = Mathf.Min(duration, settings.MaxDuration);

        if (duration <= 0f)
        {
            return;
        }

        Enemy enemy = Bot.GoalEnemy;
        LastSeenEnemyPoint = enemy != null ? enemy.EnemyPosition + (Vector3.up * REMEMBERED_HEIGHT) : null;

        BotOwner.BotsGroup.AddPointToSearch(position, SEARCH_POINT_RANGE, BotOwner);

        _flashEndTime = Time.time + duration;
        _blindFireTime = Time.time + settings.BlindFireDelay;
        ApplyModifiers(duration, settings.RecoveryPoint);
    }

    private void ApplyModifiers(float duration, float recoveryPoint)
    {
        var change = BotOwner.Settings.FileSettings.Change;

        Dismiss();
        _firstStage = Build(change);
        _secondStage = Build(change);
        BotOwner.Settings.Current.Apply(_firstStage.Modifiers, duration);
        BotOwner.Settings.Current.Apply(_secondStage.Modifiers, duration * recoveryPoint);
    }

    private static TemporaryStatModifiers Build(BotGlobalsChangeSettings change)
    {
        return new TemporaryStatModifiers(
            change.FLASH_PRECICING,
            change.FLASH_ACCURATY,
            change.FLASH_GAIN_SIGHT,
            change.FLASH_SCATTERING,
            change.FLASH_SCATTERING,
            change.FLASH_VISION_DIST,
            change.FLASH_HEARING
        );
    }

    private void Dismiss()
    {
        if (_firstStage != null)
        {
            BotOwner.Settings.Current.Dismiss(_firstStage.Modifiers);
            _firstStage = null;
        }
        if (_secondStage != null)
        {
            BotOwner.Settings.Current.Dismiss(_secondStage.Modifiers);
            _secondStage = null;
        }
    }

    public override void ManualUpdate()
    {
        if (_firstStage != null && !IsFlashed)
        {
            Dismiss();
            LastSeenEnemyPoint = null;
        }
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        Dismiss();
        base.Dispose();
    }

    private static FlashbangSettings Settings
    {
        get { return SAINPlugin.LoadedPreset.GlobalSettings.Look.Flashbang; }
    }

    private float _flashEndTime;
    private float _blindFireTime;
    private TemporaryStatModifiers _firstStage;
    private TemporaryStatModifiers _secondStage;
}
