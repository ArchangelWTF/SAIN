using System.Runtime.Serialization;

namespace SAIN.Preset.Shared.Models.WS;

public enum EPresetSyncChange
{
    Saved,
    Deleted,
    ConfigChanged,
}

[DataContract]
public sealed class SAINPresetSyncMessage
{
    [DataMember]
    public string PresetName;

    [DataMember]
    public EPresetSyncChange Change;
}
