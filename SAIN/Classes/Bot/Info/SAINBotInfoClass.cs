using EFT;
using SAIN.Components;
using SAIN.Extensions;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;
using SAIN.Preset.Shared.Personalities.BasePersonality.Categories;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.Info;

public class SAINBotInfoClass : BotComponentClassBase
{
    public SAINBotInfoClass(BotComponent sain)
        : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        Profile = new BotProfile(sain);
        WeaponInfo = new BotWeaponInfoClass(sain);
        Personality = GetPersonality(out var settings);
        PersonalitySettingsClass = settings;
        Difficulty = new BotDifficultyClass(sain);
    }

    public override void Init()
    {
        ConfigureBot(SAINPlugin.LoadedPreset);
        WeaponInfo.Init();
        Difficulty.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        WeaponInfo.ManualUpdate();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        WeaponInfo.Dispose();
        Difficulty.Dispose();
        base.Dispose();
    }

    public SAINSettingsClass FileSettings
    {
        get
        {
            if (_fileSettings == null)
            {
                _fileSettings = SAINPresetClass.Instance.BotSettings.GetSAINSettings(Profile.WildSpawnType, Profile.BotDifficulty);
            }
            return _fileSettings;
        }
    }

    public BotDifficultyClass Difficulty { get; }
    public BotProfile Profile { get; private set; }
    public BotWeaponInfoClass WeaponInfo { get; private set; }
    public EPersonality Personality { get; private set; }
    public PersonalityBehaviorSettings PersonalitySettings
    {
        get { return PersonalitySettingsClass?.Behavior; }
    }

    public PersonalitySettingsClass PersonalitySettingsClass { get; private set; }
    public float TimeBeforeSearch { get; private set; } = 0f;
    public float HoldGroundDelay { get; private set; }
    public float PercentageBeforeExtract { get; set; } = -1f;
    public bool ForceExtract { get; set; } = false;
    public float ForgetEnemyTime { get; private set; }
    public float AggressionMultiplier
    {
        get { return Difficulty.AggressionModifier; }
    }

    public void SetPersonality(EPersonality personality)
    {
        if (SAINPlugin.LoadedPreset.PersonalityManager.PersonalityDictionary.TryGetValue(personality, out var personalitySettings))
        {
            PersonalitySettingsClass = personalitySettings;
            Personality = personality;
        }
    }

    private void ConfigureBot(SAINPresetClass preset)
    {
        Personality = GetPersonality(out var settings);
        PersonalitySettingsClass = settings;
        Difficulty.UpdateSettings(preset);
        CalcTimeBeforeSearch();
        CalcHoldGroundDelay();
        UpdateExtractTime();
        FileSettings.SetConfigValues(BotOwner);
    }

    protected override void UpdatePresetSettings(SAINPresetClass preset)
    {
        ConfigureBot(preset);
        base.UpdatePresetSettings(preset);
    }

    public void CalcHoldGroundDelay()
    {
        var settings = PersonalitySettings;
        float baseTime = settings.General.HoldGroundBaseTime * AggressionMultiplier;

        float min = settings.General.HoldGroundMinRandom;
        float max = settings.General.HoldGroundMaxRandom;
        HoldGroundDelay = baseTime.Randomize(min, max).Round100();
    }

    public void CalcTimeBeforeSearch()
    {
        float searchTime;
        if (
            Profile.WildSpawnType
            is WildSpawnType.bossKilla
                or WildSpawnType.bossKillaAgro
                or WildSpawnType.bossTagilla
                or WildSpawnType.bossTagillaAgro
        )
        {
            searchTime = 0.1f;
        }
        else if (Profile.IsFollower && Bot.Squad.BotInGroup)
        {
            searchTime = 10f;
        }
        else
        {
            searchTime = PersonalitySettings.Search.SearchBaseTime;
        }

        searchTime = (searchTime.Randomize(0.66f, 1.33f) / AggressionMultiplier).Round100();
        if (searchTime < 0.1f)
        {
            searchTime = 0.1f;
        }

        TimeBeforeSearch = searchTime;
        // Get a random time between 20s and 60s
        float random = 20f.Randomize(1f, 3f).Round100();
        float forgetTime = searchTime + random;

        BotOwner.Settings.FileSettings.Mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC = forgetTime;
        ForgetEnemyTime = forgetTime;
    }

    private void UpdateExtractTime()
    {
        float percentage = Random.Range(FileSettings.Mind.MinExtractPercentage, FileSettings.Mind.MaxExtractPercentage);

        var squad = Bot?.Squad;
        var members = squad?.Members;
        if (squad != null && squad.BotInGroup && members != null && members.Count > 0)
        {
            if (squad.IAmLeader)
            {
                PercentageBeforeExtract = percentage;
                foreach (var member in members)
                {
                    var infocClass = member.Value?.Info;
                    if (infocClass != null)
                    {
                        infocClass.PercentageBeforeExtract = percentage;
                    }
                }
            }
            else if (PercentageBeforeExtract == -1f)
            {
                var Leader = squad?.LeaderComponent?.Info;
                if (Leader != null)
                {
                    PercentageBeforeExtract = Leader.PercentageBeforeExtract;
                }
            }
        }
        else
        {
            PercentageBeforeExtract = percentage;
        }
    }

    public EPersonality GetPersonality(out PersonalitySettingsClass settings)
    {
        return SAINPlugin.LoadedPreset.PersonalityManager.PersonalityDictionary.GetPersonality(this, out settings);
    }

    private SAINSettingsClass _fileSettings;
}
