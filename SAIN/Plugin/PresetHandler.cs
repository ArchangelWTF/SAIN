using System;
using System.Collections.Generic;
using System.IO;
using SAIN.Editor;
using SAIN.Preset;
using SAIN.Preset.Server;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.Preset;

namespace SAIN.Plugin;

internal class PresetHandler
{
    public const string DefaultPreset = "3. Default";
    public const string DefaultPresetDescription = "Bots are difficult but fair, the way SAIN was meant to played.";

    private static string EditorDefaultsPath
    {
        get { return Path.Combine(Path.GetDirectoryName(typeof(PresetHandler).Assembly.Location)!, "EditorDefaults.json"); }
    }

    public static event Action<SAINPresetClass> OnPresetUpdated;
    public static event Action<PresetEditorDefaults> OnEditorSettingsChanged;

    public static readonly List<SAINPresetDefinition> CustomPresetOptions = new();

    public static SAINPresetClass LoadedPreset;

    public static PresetEditorDefaults EditorDefaults;

    public static bool CanEditCurrentPreset
    {
        get
        {
            if (!ServerConfigClient.EditingAllowed)
            {
                return false;
            }
            if (ServerConfigClient.ForcedActive)
            {
                return LoadedPreset?.Info?.IsCustom == true;
            }
            return true;
        }
    }

    public static void LoadCustomPresetOptions()
    {
        CustomPresetOptions.Clear();
        foreach (var bundle in PresetSync.CustomBundles)
        {
            if (bundle?.Info != null && bundle.Info.IsCustom)
            {
                CustomPresetOptions.Add(bundle.Info);
            }
        }
    }

    public static bool Init()
    {
        PresetSyncWebSocket.Start();
        ServerConfigClient.Fetch();

        ImportEditorDefaults();
        PresetSync.PullCustomPresets();
        LoadCustomPresetOptions();

        SAINPresetDefinition presetDefinition = null;
        if (ServerConfigClient.ForcedActive)
        {
            presetDefinition = FindDefinitionByName(ServerConfigClient.ForcedPresetName);
            if (presetDefinition == null)
            {
                Logger.LogWarning($"[SAIN] Server forced preset '{ServerConfigClient.ForcedPresetName}' was not found, loading a default.");
            }
        }
        else if (!EditorDefaults.SelectedCustomPreset.IsNullOrEmpty())
        {
            CheckIfPresetLoaded(EditorDefaults.SelectedCustomPreset, out presetDefinition);
        }

        ApplyDefinition(presetDefinition);
        return LoadedPreset != null;
    }

    internal static SAINPresetDefinition FindDefinitionByName(string name)
    {
        foreach (var def in CustomPresetOptions)
        {
            if (string.Equals(def.Name, name, StringComparison.Ordinal))
            {
                return def;
            }
        }
        foreach (var def in DefaultPresetOptions())
        {
            if (string.Equals(def.Name, name, StringComparison.Ordinal))
            {
                return def;
            }
        }
        return null;
    }

    public static List<SAINPresetDefinition> DefaultPresetOptions()
    {
        var list = new List<SAINPresetDefinition>();

        foreach (var bundle in PresetSync.ServerDefaults())
        {
            if (bundle?.Info != null)
            {
                list.Add(bundle.Info);
            }
        }

        return list;
    }

    public static void SavePresetDefinition(SAINPresetDefinition definition)
    {
        if (definition.IsCustom == false)
        {
            return;
        }

        string baseName = definition.Name;

        for (int i = 0; NameTaken(definition.Name) && i < 100; i++)
        {
            definition.Name = baseName + $" Copy({i})";
        }

        if (!CustomPresetOptions.Contains(definition))
        {
            CustomPresetOptions.Add(definition);
        }
    }

    private static bool NameTaken(string name)
    {
        return PresetSync.TryGetCustomBundle(name, out _);
    }

    public static SAINPresetClass GetDefaultPreset(SAINDifficulty difficulty)
    {
        if (difficulty == SAINDifficulty.none)
        {
            return null;
        }

        if (PresetSync.TryGetServerDefault(difficulty, out var bundle))
        {
            return new SAINPresetClass(bundle);
        }

        return null;
    }

    public static void loadDefault()
    {
        LoadedPreset =
            GetDefaultPreset(EditorDefaults.SelectedDefaultPreset) ?? GetDefaultPreset(SAINDifficulty.hard) ?? GetFirstAvailablePreset();

        if (LoadedPreset == null)
        {
            throw new InvalidOperationException("No presets available from the server");
        }

        LoadedPreset.Init();
        LoadedPreset.UpdateDefaults();
    }

    private static SAINPresetClass GetFirstAvailablePreset()
    {
        foreach (var bundle in PresetSync.ServerDefaults())
        {
            if (bundle?.Info != null)
            {
                return new SAINPresetClass(bundle);
            }
        }
        foreach (var bundle in PresetSync.CustomBundles)
        {
            if (bundle?.Info != null)
            {
                return new SAINPresetClass(bundle);
            }
        }
        return null;
    }

    internal static bool IsDefaultAvailable(string name)
    {
        foreach (var bundle in PresetSync.ServerDefaults())
        {
            if (bundle?.Info != null && string.Equals(bundle.Info.Name, name, StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    internal static SAINPresetDefinition NearestAvailableDefault(SAINDifficulty target)
    {
        SAINPresetDefinition best = null;
        int bestDistance = int.MaxValue;
        foreach (var bundle in PresetSync.ServerDefaults())
        {
            if (bundle?.Info == null)
            {
                continue;
            }
            int distance = Math.Abs((int)bundle.Info.BaseSAINDifficulty - (int)target);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = bundle.Info;
            }
        }
        return best;
    }

    internal static void ApplyDefinition(SAINPresetDefinition def)
    {
        if (def != null && !def.IsCustom)
        {
            EditorDefaults.SelectedDefaultPreset = def.BaseSAINDifficulty;
        }
        InitPresetFromDefinition(def);
    }

    public static void InitPresetFromDefinition(SAINPresetDefinition def, bool isCopy = false)
    {
        if (def == null || def.IsCustom == false)
        {
            loadDefault();
            UpdateExistingBots();
            ExportEditorDefaults();
            return;
        }

        try
        {
            var defaultPreset = GetDefaultPreset(def.BaseSAINDifficulty);

            if (isCopy)
            {
                PresetSync.PublishCustom(LoadedPreset, def);
            }

            if (!PresetSync.TryGetCustomBundle(def.Name, out var bundle))
            {
                Logger.LogWarning($"[SAIN] Custom preset '{def.Name}' is not available from the server`, loading default!");
                loadDefault();
                UpdateExistingBots();
                ExportEditorDefaults();
                return;
            }

            LoadedPreset = new SAINPresetClass(bundle);
            LoadedPreset.Init();

            if (defaultPreset != null)
            {
                LoadedPreset.UpdateDefaults(defaultPreset);
            }
        }
        catch (Exception ex)
        {
            Sounds.PlaySound(EFT.UI.EUISoundType.ErrorMessage);
            Logger.LogError(ex);
            loadDefault();
        }
        UpdateExistingBots();
        ExportEditorDefaults();
    }

    public static void ExportEditorDefaults()
    {
        if (EditorDefaults.SelectedDefaultPreset == SAINDifficulty.none && LoadedPreset?.Info != null && LoadedPreset.Info.IsCustom)
        {
            EditorDefaults.SelectedCustomPreset = LoadedPreset.Info.Name;
        }
        else
        {
            EditorDefaults.SelectedCustomPreset = string.Empty;
        }

        try
        {
            string json = SPT.Common.Utils.Json.Serialize(EditorDefaults);

            if (json != _lastSavedEditorDefaults)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(EditorDefaultsPath)!);
                File.WriteAllText(EditorDefaultsPath, json);
                _lastSavedEditorDefaults = json;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to save editor defaults: {ex.Message}");
        }

        OnEditorSettingsChanged?.Invoke(EditorDefaults);
    }

    private static string _lastSavedEditorDefaults;

    public static void ImportEditorDefaults()
    {
        try
        {
            string json = File.Exists(EditorDefaultsPath) ? File.ReadAllText(EditorDefaultsPath) : null;

            if (!string.IsNullOrEmpty(json))
            {
                EditorDefaults = SPT.Common.Utils.Json.Deserialize<PresetEditorDefaults>(json);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to load editor defaults: {ex.Message}");
        }

        EditorDefaults ??= new PresetEditorDefaults(DefaultPreset);

        try
        {
            _lastSavedEditorDefaults = SPT.Common.Utils.Json.Serialize(EditorDefaults);
        }
        catch
        {
            _lastSavedEditorDefaults = null;
        }
    }

    public static void UpdateExistingBots()
    {
        OnPresetUpdated?.Invoke(LoadedPreset);
        LoadedPreset?.GlobalSettings.Update();
        LoadedPreset?.PersonalityManager.Update();
        LoadedPreset?.BotSettings.Update();
    }

    private static bool CheckIfPresetLoaded(string presetName, out SAINPresetDefinition definition)
    {
        definition = null;

        if (string.IsNullOrEmpty(presetName))
        {
            return false;
        }

        for (int i = 0; i < CustomPresetOptions.Count; i++)
        {
            var presetDef = CustomPresetOptions[i];
            if (presetDef.Name.Contains(presetName) || presetDef.Name == presetName)
            {
                definition = presetDef;
                return true;
            }
        }

        return false;
    }
}
