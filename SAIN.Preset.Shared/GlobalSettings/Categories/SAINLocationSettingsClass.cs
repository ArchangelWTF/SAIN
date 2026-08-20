using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

[DataContract]
public class SAINLocationSettingsClass : SAINSettingsBase<SAINLocationSettingsClass>, ISAINSettings
{
    public SAINLocationSettingsClass()
    {
        addNewLocations();
    }

    private void addNewLocations()
    {
        foreach (ELocation type in (ELocation[])Enum.GetValues(typeof(ELocation)))
        {
            if (LocationSettings.ContainsKey(type))
            {
                continue;
            }

            if (type == ELocation.None || type == ELocation.Terminal || type == ELocation.Town)
            {
                continue;
            }
            LocationSettings.Add(type, new DifficultySettings());
        }
    }

    [DataMember]
    [Name("Location Specific Modifiers")]
    [Description("These modifiers only apply to bots on the location they are assigned to. Applies to all bots equally.")]
    [MinMax(0.01f, 5f, 100f)]
    public Dictionary<ELocation, DifficultySettings> LocationSettings = new();

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
