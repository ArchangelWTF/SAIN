using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class DoorSettings : SAINSettingsBase<DoorSettings>, ISAINSettings
{
    //[Name("SAIN Door Handling")]
    //[Description("WIP")]
    //public bool NewDoorOpening = true;

    [DataMember]
    [Name("No Door Animations")]
    [Description(
        "Bots auto open doors instead of getting stuck in an animation, if fika is loaded, this is ignored and it is always disabled."
    )]
    public bool NoDoorAnimations = true;

    [DataMember]
    [Name("Always Push Open Doors")]
    [Description(
        "Only applies if No Door Animations is set to on. Bots will always push open doors to avoid getting stuck. Can cause cursed looking doors sometimes, but greatly improves their ability to navigate."
    )]
    public bool InvertDoors = true;

    [DataMember]
    [Name("Disable All Doors")]
    [Description("Doors are hard, just turn them all off. Only targets doors that can be open/closed normally.")]
    public bool DisableAllDoors = false;
}
