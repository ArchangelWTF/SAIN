using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.Components.RotationController;

[DataContract]
public struct TurnSettings(float smoothingValue = 0.5f, float maxTurnSpeed = 360f)
{
    [DataMember]
    [MinMax(0f, 3f, 100f)]
    public float SmoothingValue = smoothingValue;

    [DataMember]
    [MinMax(0.01f, 1000f, 100f)]
    public float MaxTurnSpeed = maxTurnSpeed;

    [DataMember]
    [Advanced]
    [Hidden]
    public EBotLookSmoothingMode SmoothingMode = EBotLookSmoothingMode.SmoothDamp;
}

public enum EBotLookMode
{
    Peace,
    Combat,
    CombatSprint,
    CombatVisibleEnemy,
    Aiming,
    RandomLook,
}

public enum EBotLookSmoothingMode
{
    Linear,
    SmoothDamp,
    SmoothDampAngle,
}
