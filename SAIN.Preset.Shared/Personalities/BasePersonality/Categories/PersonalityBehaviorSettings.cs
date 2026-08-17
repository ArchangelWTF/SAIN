using System.Runtime.Serialization;

namespace SAIN.Preset.Shared.Personalities.BasePersonality.Categories;

[DataContract]
public class PersonalityBehaviorSettings : SettingsGroupBase<PersonalityBehaviorSettings>, ISettingsGroup
{
    [DataMember]
    public PersonalityGeneralSettings General = new();

    [DataMember]
    public PersonalitySearchSettings Search = new();

    [DataMember]
    public PersonalityRushSettings Rush = new();

    [DataMember]
    public PersonalityCoverSettings Cover = new();

    [DataMember]
    public PersonalityTalkSettings Talk = new();

    public override void InitList()
    {
        SettingsList.Clear();
        SettingsList.Add(Cover);
        SettingsList.Add(General);
        SettingsList.Add(Rush);
        SettingsList.Add(Search);
        SettingsList.Add(Talk);
    }
}
