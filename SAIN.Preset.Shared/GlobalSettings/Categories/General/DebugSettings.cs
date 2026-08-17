using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class DebugOverlaySettings : SAINSettingsBase<DebugOverlaySettings>
{
    [DataMember]
    public bool Overlay_Info = true;

    [DataMember]
    public bool Overlay_Info_Expanded = false;

    [DataMember]
    public bool Overlay_Search = true;

    [DataMember]
    public bool Overlay_EnemyLists = false;

    [DataMember]
    public bool Overlay_EnemyInfo = true;

    [DataMember]
    public bool Overlay_EnemyInfo_Expanded = false;

    [DataMember]
    public bool Overlay_Decisions = false;

    [DataMember]
    public bool OverLay_AimInfo = false;

    [DataMember]
    public bool OverLay_AlwaysShowClosestHumanInfo = false;

    [DataMember]
    public bool OverLay_AlwaysShowMainPlayerInfo = false;
}

[DataContract]
public class DebugGizmoSettings : SAINSettingsBase<DebugGizmoSettings>
{
    [DataMember]
    [Name("Draw Debug Gizmos")]
    public bool DrawDebugGizmos;

    [DataMember]
    [Name("Draw Transform Gizmos")]
    public bool DrawTransformGizmos;

    [DataMember]
    [Name("Draw Player Navmesh Sampling Gizmos")]
    public bool DrawNavMeshSamplingGizmos;

    [DataMember]
    [Name("Draw Line of Sight Checks")]
    public bool DrawLineOfSightGizmos;

    [DataMember]
    [Name("Draw Volumetric Light Gizmos")]
    public bool DrawLightGizmos;

    [DataMember]
    [Name("Draw Door Links")]
    public bool DrawDoorLinks;

    [DataMember]
    [Name("Draw Recoil Gizmos")]
    public bool DebugDrawRecoilGizmos = false;

    [DataMember]
    [Name("Draw Aim Gizmos")]
    public bool DebugDrawAimGizmos = false;

    [DataMember]
    [Name("Draw Blind Corner Raycasts")]
    public bool DebugDrawBlindCorner = false;

    [DataMember]
    [Name("Draw Debug Suppression Points")]
    [Hidden]
    public bool DebugDrawProjectionPoints = false;

    [DataMember]
    [Name("Draw Search Peek Start and End Gizmos")]
    public bool DebugSearchGizmos = false;

    [Name("Draw Debug Path Safety Tester")]
    [Hidden]
    [IgnoreDataMember]
    public bool DebugDrawSafePaths = false;

    [Name("Path Safety Tester")]
    [Hidden]
    [IgnoreDataMember]
    public bool DebugEnablePathTester = false;

    [Hidden]
    [IgnoreDataMember]
    public bool DebugMovementPlan = false;
}

[DataContract]
public class DebugLogSettings : SAINSettingsBase<DebugLogSettings>
{
    [DataMember]
    [Name("Global Debug Mode")]
    public bool GlobalDebugMode;

    [DataMember]
    [Name("Global Performance Profiling Mode")]
    [Description("Enables function sampling for Unity Profiling.")]
    public bool GlobalProfilingToggle;

    [DataMember]
    [Name("Test Bot Sprint Pathfinder")]
    public bool ForceBotsToRunAround;

    [DataMember]
    [Name("Test Bot Crawling")]
    public bool ForceBotsToTryCrawl;

    [DataMember]
    [Name("Test Grenade Throw")]
    public bool TestGrenadeThrow;

    [DataMember]
    [Name("Draw Debug Labels")]
    public bool DrawDebugLabels;

    [DataMember]
    [Name("Debug External")]
    public bool DebugExternal;

    [DataMember]
    [Name("Debug Recoil Calculations")]
    public bool DebugRecoilCalculations = false;

    [DataMember]
    [Name("Debug Aim Calculations")]
    public bool DebugAimCalculations = false;

    [DataMember]
    [Name("Debug Hearing Calc Results")]
    public bool DebugHearing = false;

    [DataMember]
    [Name("Debug Extracts")]
    public bool DebugExtract = false;

    [Name("Collect and Export Bot Layer and Brain Info")]
    [Hidden]
    [IgnoreDataMember]
    public bool CollectBotLayerBrainInfo = false;
}

[DataContract]
public class DebugSettings : SAINSettingsBase<DebugSettings>, ISAINSettings
{
    public DebugSettings()
    {
        Instance = this;
    }

    public static DebugSettings Instance { get; private set; }

    [DataMember]
    public DebugLogSettings Logs = new();

    [DataMember]
    public DebugGizmoSettings Gizmos = new();

    [DataMember]
    public DebugOverlaySettings Overlay = new();

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(Logs);
        list.Add(Gizmos);
        list.Add(Overlay);
    }
}
