global using EFTMath = MyExtensions;
using BepInEx;
using BepInEx.Configuration;
using SAIN.Components.BotController;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.Server;
using SAIN.Preset.Shared;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings.Categories.General;
using SPT.Reflection.Patching;
using UnityEngine;
using static SAIN.AssemblyInfoClass;

namespace SAIN;

[BepInPlugin(SAINGUID, SAINName, SAINVersionInfo.SAINVersion)]
[BepInDependency(BigBrainGUID, BigBrainVersion)]
//[BepInDependency(SPTGUID, SPTVersion)]
[BepInProcess(EscapeFromTarkov)]
[BepInIncompatibility("com.dvize.BushNoESP")]
[BepInIncompatibility("com.dvize.NoGrenadeESP")]
public class SAINPlugin : BaseUnityPlugin
{
    private PatchManager _patchManager;

    public static DebugSettings DebugSettings
    {
        get { return LoadedPreset.GlobalSettings.General.Debug; }
    }

    public static bool DebugMode
    {
        get { return DebugSettings.Logs.GlobalDebugMode; }
    }

    public static bool ProfilingMode
    {
        get { return DebugSettings.Logs.GlobalProfilingToggle; }
    }

    public static bool DrawDebugGizmos
    {
        get { return DebugSettings.Gizmos.DrawDebugGizmos; }
    }

    public static PresetEditorDefaults EditorDefaults
    {
        get { return PresetHandler.EditorDefaults; }
    }

    public static ECombatDecision ForceSoloDecision = ECombatDecision.None;

    public static ESquadDecision ForceSquadDecision = ESquadDecision.None;

    public static ESelfActionType ForceSelfDecision = ESelfActionType.None;

    public void Awake()
    {
        _patchManager = new(this, true);

        if (!PresetHandler.Init() || !EFTCoreSettings.Load())
        {
            Logger.LogError(
                "[SAIN] Startup aborted: could not load the presets and core overrides from the SAIN server mod, re-install SAIN correctly and restart."
            );

            return;
        }
        BotSpawnController.LoadExclusionList();
        BindConfigs();
        _patchManager.EnablePatches();
        BigBrainHandler.Init();
    }

    private void BindConfigs()
    {
        string category = "SAIN Editor";
        OpenEditorButton = Config.Bind(category, "Open Editor", false, "Opens the Editor on press");
        OpenEditorConfigEntry = Config.Bind(
            category,
            "Open Editor Shortcut",
            new KeyboardShortcut(KeyCode.F6),
            "The keyboard shortcut that toggles editor"
        );
    }

    public static ConfigEntry<bool> OpenEditorButton { get; private set; }

    public static ConfigEntry<KeyboardShortcut> OpenEditorConfigEntry { get; private set; }

    public static SAINPresetClass LoadedPreset
    {
        get { return PresetHandler.LoadedPreset; }
    }

    public void Update()
    {
        PresetSyncWebSocket.Resync();

        ModDetection.ManualUpdate();
        SAINEditor.ManualUpdate();
        DebugGizmos.ManualUpdate();
    }

    public void Start()
    {
        SAINEditor.Init();
    }

    public void OnDestroy()
    {
        PresetSyncWebSocket.Instance?.Close();
    }

    public void LateUpdate()
    {
        SAINEditor.LateUpdate();
    }

    public void OnGUI()
    {
        SAINEditor.OnGUI();
    }
}
