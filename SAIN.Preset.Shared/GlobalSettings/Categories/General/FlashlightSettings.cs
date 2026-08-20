using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class FlashlightSettings : SAINSettingsBase<FlashlightSettings>, ISAINSettings
{
    [DataMember]
    [MinMax(0.25f, 10f, 100f)]
    public float DazzleEffectiveness = 3f;

    [DataMember]
    [MinMax(0f, 60f)]
    public float MaxDazzleRange = 40f;

    [DataMember]
    public bool AllowLightOnForDarkBuildings = true;

    [DataMember]
    public bool TurnLightOffNoEnemyPMC = true;

    [DataMember]
    public bool TurnLightOffNoEnemySCAV = false;

    [DataMember]
    public bool TurnLightOffNoEnemyGOONS = true;

    [DataMember]
    public bool TurnLightOffNoEnemyBOSS = false;

    [DataMember]
    public bool TurnLightOffNoEnemyFOLLOWER = false;

    [DataMember]
    public bool TurnLightOffNoEnemyRAIDERROGUE = false;

    [DataMember]
    [Advanced]
    public bool DebugFlash = false;

    [DataMember]
    public bool SillyMode = false;
}
