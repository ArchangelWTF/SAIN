using System;
using System.Collections.Generic;
using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Models.Structs;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using EFT.Ballistics;

namespace SAIN.Classes.Bot.Sense.Hearing;

public class HearingInput(HearingSensor hearingSensor) : BotSubClass<HearingSensor>(hearingSensor), IBotClass
{
    private const float BOT_DEAF_TIME_INTERVAL = 0.75f;
    private const float DeafenCoef_Gunfire = 0.33f;
    private const float DeafenCoef_Suppressed = 0.33f;
    private const float DeafenCoef_Convo = 0.4f;
    private const float DeafenCoef_Generic = 0.3f;

    private const float IMPACT_HEAR_FREQUENCY = 0.5f;
    private const float IMPACT_HEAR_FREQUENCY_FAR = 0.05f;
    private const float IMPACT_MAX_HEAR_DISTANCE = 50f * 50f;
    private const float IMPACT_DISPERSION = 5f * 5f;

    private bool _ignoreUnderFire = false;
    private bool _ignoreHearing = false;

    private float _botDeafenedUntilTime = -1f;

    public bool IsBotDeafened
    {
        get { return Time.time < _botDeafenedUntilTime; }
    }

    public override void Init()
    {
        PlayerComponent.OnBulletFlyBy += OnBulletFlyBy;
        BotManagerComponent.Instance.BotHearing.BulletImpact += BulletImpacted;
        base.Init();
    }

    protected void OnBulletFlyBy(PlayerComponent Source, Shot Bullet)
    {
        Enemy Enemy = Bot.EnemyController.CheckAddEnemy(Source.Player);
        if (Enemy != null)
        {
            SoundEvent SoundEvent = new(SAINSoundType.BulletImpact, Source.Position, Source, 100, 1, 999);
            AISoundData Sound = new(SoundEvent, Bot, PlayerComponent.GetDistanceToPlayer(Source.ProfileId), Enemy);
            Vector3 BulletPosition = Bullet.CurrentPosition;
            Vector3 MyHeadPosition = Bot.Transform.HeadData.HeadPosition;
            float Dist = (BulletPosition - MyHeadPosition).magnitude;
            BaseClass.ReactToBulletFlyBy(Sound, Dist);
        }
    }

    public override void ManualUpdate()
    {
        CheckResetHearing();
        base.ManualUpdate();
    }

    public event Action<AISoundData> OnFriendlySoundHeard;

    public readonly List<AISoundData> SoundDataToReactTo = [];
    public readonly List<AISoundData> AISoundCachedEvents = [];
    public readonly List<AISoundData> AISoundCachedEvents_Conversations = [];
    public readonly List<AISoundData> AISoundCachedEvents_Gunshots = [];
    public readonly List<AISoundData> AISoundCachedEvents_Gunshots_Suppressed = [];

    public bool IsIgnoringSounds(bool includingGunfire = true)
    {
        if (!Bot.BotActive)
        {
            return true;
        }

        if (Bot.GameEnding)
        {
            return true;
        }

        if (_ignoreHearing && (_ignoreUnderFire || !includingGunfire))
        {
            return true;
        }

        return false;
    }

    public void CheckAddSoundToCache(SoundEvent Sound, float PlayerDistance)
    {
        if (Bot.Hearing.HearingInput.IsIgnoringSounds(Sound.SoundType.IsGunShot()))
        {
            return;
        }

        if (Bot.EnemyController != null)
        {
            Enemy enemy = Bot.EnemyController.CheckAddEnemy(Sound.GetPlayer());

            if (enemy == null && Sound.SoundType != SAINSoundType.Conversation)
            {
                return;
            }

            AISoundData Data = new(Sound, Bot, PlayerDistance, enemy);

            switch (Sound.SoundType)
            {
                case SAINSoundType.Shot:
                    AISoundCachedEvents_Gunshots.Add(Data);
                    break;

                case SAINSoundType.SuppressedShot:
                    AISoundCachedEvents_Gunshots_Suppressed.Add(Data);
                    break;

                case SAINSoundType.Conversation:
                    AISoundCachedEvents_Conversations.Add(Data);
                    break;

                default:
                    AISoundCachedEvents.Add(Data);
                    break;
            }
        }
    }

    public void ProcessAISoundCache()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Process Sounds For Bots");

        bool wasDeafened = IsBotDeafened;
        bool deafeningSoundHeard = false;

        // Process gunshots first, since they can trigger a bot to be deaf to other sounds
        if (
            AISoundCachedEvents_Gunshots.Count > 0
            && ProcessGunshots(AISoundCachedEvents_Gunshots, wasDeafened, DeafenCoef_Gunfire, SoundDataToReactTo)
        )
        {
            deafeningSoundHeard = true;
            wasDeafened = true;
        }

        if (
            AISoundCachedEvents_Gunshots_Suppressed.Count > 0
            && ProcessGunshots(AISoundCachedEvents_Gunshots_Suppressed, wasDeafened, DeafenCoef_Suppressed, SoundDataToReactTo)
        )
        {
            deafeningSoundHeard = true;
            wasDeafened = true;
        }

        // Process most sounds if we aren't deafened
        if (AISoundCachedEvents_Conversations.Count > 0)
        {
            ProcessSounds(AISoundCachedEvents_Conversations, wasDeafened, DeafenCoef_Convo, SoundDataToReactTo);
        }

        if (AISoundCachedEvents.Count > 0)
        {
            ProcessSounds(AISoundCachedEvents, wasDeafened, DeafenCoef_Generic, SoundDataToReactTo);
        }

        ProcessSoundReactions();

        if (deafeningSoundHeard)
        {
            DeafenBot(BOT_DEAF_TIME_INTERVAL);
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    private void ProcessSoundReactions()
    {
        bool soundRemoved = false;

        for (int i = SoundDataToReactTo.Count - 1; i >= 0; i--)
        {
            AISoundData sound = SoundDataToReactTo[i];

            if (!sound.CanReport(0.2f))
            {
                continue;
            }

            TryReactToSound(sound);
            SoundDataToReactTo.RemoveAt(i);
            soundRemoved = true;
        }

        if (soundRemoved)
        {
            SoundDataToReactTo.TrimExcess();
        }
    }

    private void DeafenBot(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        float deafenedUntilTime = Time.time + duration;

        if (deafenedUntilTime > _botDeafenedUntilTime)
        {
            _botDeafenedUntilTime = deafenedUntilTime;
        }
    }

    private void TryReactToSound(AISoundData Sound)
    {
        if (Sound.Enemy != null)
        {
            BaseClass.ReactToHeardSound(Sound);
        }
        else
        {
            OnFriendlySoundHeard?.Invoke(Sound);
        }
    }

    private static bool ProcessSounds(List<AISoundData> Sounds, bool PreviouslyDeaf, float DeafenCoef, List<AISoundData> Results)
    {
        bool DeafeningShot = false;
        int Count = Sounds.Count;
        if (Count > 0)
        {
            Sounds.Sort((a, b) => a.PlayerDistance.CompareTo(b.PlayerDistance));
            for (int i = 0; i < Count; i++)
            {
                AISoundData Sound = Sounds[i];
                // If Sounds is closer than or equal to the input fraction of the Baserange of this sound, always report it. If we are checking gunshots, then this sound will deafen the bot for a duration.
                if (Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume)
                {
                    if (PreviouslyDeaf && Sound.PlayerDistance > Sound.Sound.BaseRangeWithVolume * DeafenCoef)
                    {
                        continue;
                    }

                    Results.Add(Sound);
                }
            }
            Sounds.Clear();
        }
        return DeafeningShot;
    }

    private static bool ProcessGunshots(List<AISoundData> Sounds, bool previouslyDeaf, float DeafenCoef, List<AISoundData> Results)
    {
        bool DeafeningShot = false;
        int Count = Sounds.Count;
        if (Count > 0)
        {
            Sounds.Sort((a, b) => a.PlayerDistance.CompareTo(b.PlayerDistance));
            for (int i = 0; i < Count; i++)
            {
                AISoundData Sound = Sounds[i];
                // If Sounds is closer than or equal to the input fraction of the Baserange of this sound, always report it. If we are checking gunshots, then this sound will deafen the bot for a duration.
                if (Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume)
                {
                    bool thisShotDeafened = Sound.PlayerDistance <= Sound.Sound.BaseRangeWithVolume * DeafenCoef;
                    if (previouslyDeaf && !thisShotDeafened)
                    {
                        continue;
                    }

                    if (!DeafeningShot)
                    {
                        DeafeningShot = thisShotDeafened;
                    }

                    Results.Add(Sound);
                }
            }
            Sounds.Clear();
        }
        return DeafeningShot;
    }

    private void CheckResetHearing()
    {
        if (!_ignoreHearing)
        {
            if (_ignoreUnderFire)
            {
                _ignoreUnderFire = false;
            }

            return;
        }
        if (_ignoreUntilTime > 0 && _ignoreUntilTime < Time.time)
        {
            _ignoreHearing = false;
            _ignoreUnderFire = false;
            return;
        }
        if (Bot.EnemyController.VisibleEnemies.Count > 0)
        {
            _ignoreHearing = false;
            _ignoreUnderFire = false;
            return;
        }
    }

    public override void Dispose()
    {
        BotManagerComponent.Instance.BotHearing.BulletImpact -= BulletImpacted;
        PlayerComponent.OnBulletFlyBy -= OnBulletFlyBy;
        base.Dispose();
    }

    private void BulletImpacted(Shot bullet)
    {
        if (IsIgnoringSounds(true))
        {
            return;
        }

        float currentTime = Time.time;

        if (_nextHearImpactTime > currentTime)
        {
            return;
        }

        if (Bot.HasEnemy)
        {
            return;
        }

        var player = bullet.Player?.iPlayer;
        if (player == null)
        {
            return;
        }

        var enemy = Bot.EnemyController.GetEnemy(player.ProfileId, true);
        if (enemy == null)
        {
            return;
        }

        if (!SoundListenerStarted(enemy.EnemyPlayerComponent))
        {
            return;
        }

        if (Bot.PlayerComponent.AIData.PlayerLocation.InBunker != enemy.EnemyPlayerComponent.AIData.PlayerLocation.InBunker)
        {
            return;
        }

        float distance = (bullet.CurrentPosition - Bot.Position).sqrMagnitude;

        if (distance > IMPACT_MAX_HEAR_DISTANCE)
        {
            _nextHearImpactTime = currentTime + IMPACT_HEAR_FREQUENCY_FAR;
            return;
        }
        _nextHearImpactTime = currentTime + IMPACT_HEAR_FREQUENCY;

        float dispersion = distance / IMPACT_DISPERSION;
        Vector3 random = UnityEngine.Random.onUnitSphere;
        random.y = 0;
        random = random.normalized * dispersion;
        Vector3 estimatedPos = enemy.EnemyPosition + random;

        SAINHearingReport report = new()
        {
            position = estimatedPos,
            soundType = SAINSoundType.BulletImpact,
            placeType = EEnemyPlaceType.Hearing,
            isDanger = distance < 25f * 25f,
            shallReportToSquad = true,
        };

        enemy.Hearing.SetHeard(report, currentTime);
    }

    private bool SoundListenerStarted(PlayerComponent player)
    {
        if (!player.IsAI)
        {
            return true;
        }

        if (!_hearingStarted)
        {
            if (!PlayerComponent.AIData.AISoundPlayer.SoundMakerStarted)
            {
                return false;
            }
            _hearingStarted = true;
        }

        return true;
    }

    private bool _hearingStarted;

    public bool SetIgnoreHearingExternal(bool value, bool ignoreUnderFire, float duration, out string reason)
    {
        // Only allow the bot to ignore hearing if it's not in combat
        if (value)
        {
            if (Bot.GoalEnemy?.IsVisible == true)
            {
                reason = "Enemy Visible";
                return false;
            }
            if (BotOwner.Memory.IsUnderFire && !ignoreUnderFire)
            {
                reason = "Under Fire";
                return false;
            }
        }

        _ignoreUnderFire = ignoreUnderFire;
        _ignoreHearing = value;

        if (value && duration > 0f)
        {
            _ignoreUntilTime = Time.time + duration;
        }
        else
        {
            _ignoreUntilTime = -1f;
        }

        reason = string.Empty;
        return true;
    }

    private float _nextHearImpactTime;
    private float _ignoreUntilTime;
}
