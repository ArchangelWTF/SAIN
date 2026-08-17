using System.Runtime.Serialization;
using SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;
using SAIN.Preset.Shared.Personalities;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings;

[DataContract]
public class SAINSettingsClass : SettingsGroupBase<SAINSettingsClass>
{
    [DataMember]
    public DifficultySettings Difficulty = new();

    [DataMember]
    public SAINCoreSettings Core = new();

    [DataMember]
    public SAINAimingSettings Aiming = new();

    [DataMember]
    public SAINBossSettings Boss = new();

    [DataMember]
    public SAINChangeSettings Change = new();

    [DataMember]
    public SAINGrenadeSettings Grenade = new();

    [DataMember]
    public SAINHearingSettings Hearing = new();

    [DataMember]
    public SAINLaySettings Lay = new();

    [DataMember]
    public SAINLookSettings Look = new();

    [DataMember]
    public SAINMindSettings Mind = new();

    [DataMember]
    public SAINMoveSettings Move = new();

    [DataMember]
    public SAINPatrolSettings Patrol = new();

    [DataMember]
    public SAINScatterSettings Scattering = new();

    [DataMember]
    public SAINShootSettings Shoot = new();

    public override void InitList()
    {
        SettingsList.Clear();
        SettingsList.Add(Difficulty);
        SettingsList.Add(Core);
        SettingsList.Add(Aiming);
        SettingsList.Add(Boss);
        SettingsList.Add(Change);
        SettingsList.Add(Grenade);
        SettingsList.Add(Hearing);
        SettingsList.Add(Lay);
        SettingsList.Add(Look);
        SettingsList.Add(Mind);
        SettingsList.Add(Patrol);
        SettingsList.Add(Scattering);
        SettingsList.Add(Shoot);
    }
}
