using System.Runtime.Serialization;

namespace SAIN.Preset.Shared;

[DataContract]
public class CoreOverrides
{
    [DataMember]
    public static readonly string README =
        "Dont change anything here unless you know exactly what you are doing. Changes here require game restart! Not all settings do what the name suggests.";

    [DataMember]
    public bool SCAV_GROUPS_TOGETHER = false;

    [DataMember]
    public float DIST_NOT_TO_GROUP = 50f;

    [DataMember]
    public bool CAN_SHOOT_TO_HEAD = true;

    [DataMember]
    public float SOUND_DOOR_OPEN_METERS = 40f;

    [DataMember]
    public float SOUND_DOOR_BREACH_METERS = 70f;

    [DataMember]
    public float JUMP_SPREAD_DIST = 65f;

    [DataMember]
    public float BASE_WALK_SPEREAD2 = 65f;

    [DataMember]
    public int GRENADE_PRECISION = 10;

    [DataMember]
    public float PRONE_POSE = 1f;

    [DataMember]
    public float MOVE_COEF = 1f;

    [DataMember]
    public float LOWER_POSE = 1f;

    [DataMember]
    public float MAX_POSE = 1f;

    [DataMember]
    public float FLARE_POWER = 1.75f;

    [DataMember]
    public float FLARE_TIME = 2.5f;

    [DataMember]
    public float SHOOT_TO_CHANGE_RND_PART_DELTA = 2f;
}
