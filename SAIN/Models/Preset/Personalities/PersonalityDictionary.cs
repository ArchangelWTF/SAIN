using System.Collections.Generic;
using EFT;
using SAIN.Extensions;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;
using SAIN.SAINComponent.Classes.Info;
using SPT.Common.Http;
using SPT.Common.Utils;
using UnityEngine;

namespace SAIN.Models.Preset.Personalities;

public sealed class PersonalityDictionary : Dictionary<EPersonality, PersonalitySettingsClass>
{
    static PersonalityDictionary()
    {
        if (_nicknames == null)
        {
            var nicknamesJson = RequestHandler.GetJson("/sain/namepersonalities");
            _nicknames = Json.Deserialize<NicknamesModel>(nicknamesJson);
        }
    }

    private static readonly NicknamesModel _nicknames;

    public EPersonality GetPersonality(SAINBotInfoClass infoClass, out PersonalitySettingsClass settings)
    {
        if (checkForcePersonality(out EPersonality result))
        {
            settings = this[result];
            return result;
        }

        result = setNicknamePersonality(infoClass.Profile.NickName);
        if (result != EPersonality.Normal)
        {
            settings = this[result];
            return result;
        }

        result = setBossPersonality(infoClass.Profile.WildSpawnType);
        if (result != EPersonality.Normal)
        {
            settings = this[result];
            return result;
        }

        foreach (var setting in this)
        {
            if (canBotBePersonality(infoClass, setting.Key))
            {
                settings = setting.Value;
                return setting.Key;
            }
        }

        if (infoClass.Profile.IsPMC && EFTMath.RandomBool(33))
        {
            result = EPersonality.Chad;
        }
        else
        {
            result = EPersonality.Normal;
        }

        settings = this[result];
        return result;
    }

    public PersonalitySettingsClass GetSettings(EPersonality personality)
    {
        if (this.TryGetValue(personality, out var result))
        {
            return result;
        }
        return null;
    }

    private bool checkForcePersonality(out EPersonality personality)
    {
        foreach (var item in SAINPlugin.LoadedPreset.GlobalSettings.Mind.ForcePersonality)
        {
            if (item.Value == true)
            {
                personality = item.Key;
                return true;
            }
        }
        personality = EPersonality.Normal;
        return false;
    }

    private EPersonality setNicknamePersonality(string nickname)
    {
        if (nickname.IsNullOrEmpty())
        {
            return EPersonality.Normal;
        }
        string lowerNick = nickname.ToLower();
        foreach (KeyValuePair<string, EPersonality> kvp in _nicknames.NicknamePersonalities)
        {
            if (lowerNick.Contains(kvp.Key.ToLower()))
            {
                return kvp.Value;
            }
        }
        return EPersonality.Normal;
    }

    private EPersonality setBossPersonality(WildSpawnType wildSpawnType)
    {
        if (GlobalSettingsClass.Instance.Mind.PERS_BOSSES.TryGetValue(wildSpawnType.ToESain(), out EPersonality bossPersonality))
        {
            return bossPersonality;
        }
        return EPersonality.Normal;
    }

    private bool canBotBePersonality(SAINBotInfoClass infoClass, EPersonality personality)
    {
        if (!this.TryGetValue(personality, out var settings))
        {
            return false;
        }
        var assignment = settings.Assignment;
        if (!assignment.Enabled)
        {
            return false;
        }
        if (checkRandomAssignment(settings))
        {
            return true;
        }
        if (meetsRequirements(infoClass, settings))
        {
            float assignmentChance = getChance(infoClass.Profile.PowerLevel, settings);
            if (EFTMath.RandomBool(assignmentChance))
            {
                return true;
            }
        }
        return false;
    }

    private bool checkRandomAssignment(PersonalitySettingsClass settings)
    {
        return settings.Assignment.CanBeRandomlyAssigned && EFTMath.RandomBool(settings.Assignment.RandomlyAssignedChance);
    }

    private bool meetsRequirements(SAINBotInfoClass infoClass, PersonalitySettingsClass settings)
    {
        var assignment = settings.Assignment;
        return assignment.AllowedTypes.Contains(infoClass.Profile.WildSpawnType.ToESain())
            && infoClass.Profile.PowerLevel <= assignment.PowerLevelMax
            && infoClass.Profile.PowerLevel > assignment.PowerLevelMin
            && infoClass.Profile.PlayerLevel <= assignment.MaxLevel
            && infoClass.Profile.PlayerLevel > assignment.MinLevel;
    }

    private float getChance(float powerLevel, PersonalitySettingsClass settings)
    {
        var assignment = settings.Assignment;
        powerLevel = Mathf.Clamp(powerLevel, 0, 1000);
        float modifier0to1 =
            (powerLevel - assignment.PowerLevelScaleStart) / (assignment.PowerLevelScaleEnd - assignment.PowerLevelScaleStart);
        if (assignment.InverseScale)
        {
            modifier0to1 = 1f - modifier0to1;
        }
        float result = assignment.MaxChanceIfMeetRequirements * modifier0to1;
        result = Mathf.Clamp(result, 0f, 100f);
        //Logger.LogDebug($"Result: [{result}] Power: [{powerLevel}] PowerLevelScaleStart [{PowerLevelScaleStart}] PowerLevelScaleEnd [{PowerLevelScaleEnd}] MaxChanceIfMeetRequirements [{MaxChanceIfMeetRequirements}]");
        return result;
    }
}
