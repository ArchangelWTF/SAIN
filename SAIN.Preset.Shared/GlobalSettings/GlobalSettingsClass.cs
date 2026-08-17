using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings.Categories;
using SAIN.Preset.Shared.GlobalSettings.Categories.General;
using SAIN.Preset.Shared.GlobalSettings.Categories.Look;
using SAIN.Preset.Shared.Personalities;

namespace SAIN.Preset.Shared.GlobalSettings;

[DataContract]
public class GlobalSettingsClass : SettingsGroupBase<GlobalSettingsClass>
{
    [Hidden]
    [IgnoreDataMember]
    public static GlobalSettingsClass Instance;

    public GlobalSettingsClass()
    {
        Instance = this;
    }

    public override void Init()
    {
        InitList();
        CreateDefaults();
        Update();
    }

    [DataMember]
    public DifficultySettings Difficulty = new();

    [DataMember]
    public GeneralSettings General = new();

    [DataMember]
    public AimSettings Aiming = new();

    [DataMember]
    public HearingSettings Hearing = new();

    [DataMember]
    public SAINLocationSettingsClass Location = new();

    [DataMember]
    public LookSettings Look = new();

    [DataMember]
    public MindSettings Mind = new();

    [DataMember]
    public MoveSettings Move = new();

    [DataMember]
    public SteeringSettings Steering = new();

    [DataMember]
    public ShootSettings Shoot = new();

    [DataMember]
    public TalkSettings Talk = new();

    [DataMember]
    [Name("Squad Talk")]
    public SquadTalkSettings SquadTalk = new();

    [DataMember]
    [Name("Power Level Calculation")]
    public PowerCalcSettings PowerCalc = new();

    public override void InitList()
    {
        SettingsList.Clear();

        Difficulty.Init(SettingsList);
        General.Init(SettingsList);
        Aiming.Init(SettingsList);
        Hearing.Init(SettingsList);
        Location.Init(SettingsList);
        Look.Init(SettingsList);
        Mind.Init(SettingsList);
        Move.Init(SettingsList);
        Shoot.Init(SettingsList);
        Talk.Init(SettingsList);
        SquadTalk.Init(SettingsList);
        PowerCalc.Init(SettingsList);
        Steering.Init(SettingsList);
    }
}
