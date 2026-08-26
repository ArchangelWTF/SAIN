using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.BotSettings;

[DataContract]
public sealed class BotTypeInfo
{
    [DataMember]
    public string Name;

    [DataMember]
    public ESainWildSpawnType WildSpawnType;

    [DataMember]
    public float DefaultDifficultyModifier = 0.5f;

    [DataMember]
    public string Section;

    [DataMember]
    public string Description;
    
    [DataMember]
    public string BotDbKey;

    [DataMember]
    public List<string> BrainsToApply;

    [DataMember]
    public List<string> LayersToRemove;

    [DataMember]
    public string BaseBrain;

    public BotTypeInfo() { }

    public BotTypeInfo(
        string name,
        ESainWildSpawnType type,
        float defaultDifficultyModifier,
        string section = null,
        string description = null
    )
    {
        Name = name;
        WildSpawnType = type;
        DefaultDifficultyModifier = defaultDifficultyModifier;
        Section = section;
        Description = description;
    }
}
