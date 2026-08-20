using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAIN.Extensions;
using SAIN.Plugin;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.Models.WS;
using SAIN.Preset.Shared.Preset;
using SharedWildSpawnType = SAIN.Preset.Shared.Enums.ESainWildSpawnType;

namespace SAIN.Preset.Server;

public static class PresetSync
{
    private static readonly JsonSerializerSettings _settings = new()
    {
        Converters = { new StringEnumConverter() },
        Formatting = Formatting.Indented,
    };

    private static bool InRaid
    {
        get { return SAIN.Components.GameWorldComponent.Instance != null; }
    }

    private static List<SAINPresetBundle> _serverDefaults;
    private static bool _serverDefaultsFetched;
    private static bool _liveReapplyPending;
    private static bool _configReapplyPending;
    private static SAINPresetDefinition _pendingConfigTarget;

    private static readonly Dictionary<string, SAINPresetBundle> _customBundles = new(StringComparer.Ordinal);

    public static IEnumerable<SAINPresetBundle> CustomBundles
    {
        get { return _customBundles.Values; }
    }

    public static bool TryGetCustomBundle(string name, out SAINPresetBundle bundle)
    {
        return _customBundles.TryGetValue(name, out bundle);
    }

    public static List<SAINPresetBundle> ServerDefaults()
    {
        if (_serverDefaultsFetched)
        {
            return _serverDefaults ?? [];
        }
        _serverDefaultsFetched = true;
        try
        {
            string json = ServerPresetClient.GetGeneratedDefaults();
            if (!string.IsNullOrEmpty(json) && json != "null")
            {
                _serverDefaults = JsonConvert.DeserializeObject<List<SAINPresetBundle>>(json, _settings);
                Logger.LogInfo($"[SAIN] Fetched {_serverDefaults?.Count ?? 0} server-generated default presets.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to fetch server-generated default presets: {ex.Message}");
        }
        return _serverDefaults ?? [];
    }

    public static void RefreshServerDefaults()
    {
        _serverDefaults = null;
        _serverDefaultsFetched = false;
        ServerDefaults();
    }

    public static bool TryGetServerDefault(SAINDifficulty difficulty, out SAINPresetBundle bundle)
    {
        bundle = null;
        foreach (var b in ServerDefaults())
        {
            if (b?.Info != null && b.Info.BaseSAINDifficulty == difficulty)
            {
                bundle = b;
                return true;
            }
        }
        return false;
    }

    public static void PullCustomPresets()
    {
        try
        {
            _customBundles.Clear();
            foreach (string name in ServerPresetClient.ListCustom())
            {
                try
                {
                    string json = ServerPresetClient.GetCustom(name);
                    if (string.IsNullOrEmpty(json) || json == "null")
                    {
                        continue;
                    }
                    var bundle = JsonConvert.DeserializeObject<SAINPresetBundle>(json, _settings);
                    if (bundle?.Info == null)
                    {
                        continue;
                    }
                    _customBundles[bundle.Info.Name] = bundle;
                    Logger.LogInfo($"[SAIN] Pulled custom preset '{name}' from server.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[SAIN] Failed to pull custom preset '{name}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug($"[SAIN] Custom preset pull skipped: {ex.Message}");
        }
    }

    public static void ProcessDeferred()
    {
        if (InRaid)
        {
            return;
        }

        if (_configReapplyPending)
        {
            _configReapplyPending = false;
            var target = _pendingConfigTarget;
            _pendingConfigTarget = null;
            Logger.LogInfo("[SAIN] Applying the server preset change from during the raid.");
            PresetHandler.ApplyDefinition(target);
        }

        if (!_liveReapplyPending)
        {
            return;
        }
        _liveReapplyPending = false;

        string name = PresetHandler.LoadedPreset?.Info?.Name;
        if (name != null && TryGetCustomBundle(name, out var bundle))
        {
            Logger.LogInfo($"[SAIN] Applying preset '{name}' that changed during the raid.");
            PresetHandler.InitPresetFromDefinition(bundle.Info);
        }
        else
        {
            Logger.LogInfo("[SAIN] The preset in use was removed during the raid, loading a default.");
            PresetHandler.InitPresetFromDefinition(null);
        }
    }

    /// <summary>Re-reads the admin server config live and, if a preset is forced or presets are off, applies it.</summary>
    public static void ApplyRemoteConfigChange()
    {
        ServerConfigClient.Fetch();
        RefreshServerDefaults();
        Logger.LogInfo("[SAIN] Server config changed.");

        SAINPresetDefinition target;
        if (ServerConfigClient.ForcedActive)
        {
            target = PresetHandler.FindDefinitionByName(ServerConfigClient.ForcedPresetName);
        }
        else
        {
            // Not forced: only step in if the loaded default was just hidden, then move to the nearest one.
            var loaded = PresetHandler.LoadedPreset?.Info;
            if (loaded == null || loaded.IsCustom || PresetHandler.IsDefaultAvailable(loaded.Name))
            {
                return;
            }
            target = PresetHandler.NearestAvailableDefault(loaded.BaseSAINDifficulty);
            Logger.LogInfo($"[SAIN] Preset '{loaded.Name}' was hidden by the server, falling back to '{target?.Name ?? "a default"}'.");
        }

        string currentName = PresetHandler.LoadedPreset?.Info?.Name;
        if (string.Equals(currentName, target?.Name, StringComparison.Ordinal))
        {
            return;
        }

        if (InRaid)
        {
            _configReapplyPending = true;
            _pendingConfigTarget = target;
            Logger.LogInfo("[SAIN] A server preset change will apply when the raid ends.");
            return;
        }

        PresetHandler.ApplyDefinition(target);
    }

    private static void ReapplyLivePreset(SAINPresetDefinition info)
    {
        if (InRaid)
        {
            _liveReapplyPending = true;
            Logger.LogInfo($"[SAIN] Preset '{info?.Name}' changed, but a raid is running — applying when it ends.");
            return;
        }
        PresetHandler.InitPresetFromDefinition(info);
    }

    public static void ResyncCustomPresets()
    {
        string loadedName = PresetHandler.LoadedPreset?.Info?.Name;
        bool loadedIsCustom = PresetHandler.LoadedPreset?.Info?.IsCustom == true;

        PullCustomPresets();
        PresetHandler.LoadCustomPresetOptions();

        if (loadedIsCustom && loadedName != null && TryGetCustomBundle(loadedName, out var bundle))
        {
            ReapplyLivePreset(bundle.Info);
        }
    }

    public static void ApplyRemoteChange(SAINPresetSyncMessage message)
    {
        string name = message.PresetName;
        bool isLoaded = string.Equals(PresetHandler.LoadedPreset?.Info?.Name, name, StringComparison.Ordinal);

        if (message.Change == EPresetSyncChange.Deleted)
        {
            _customBundles.Remove(name);
            PresetHandler.LoadCustomPresetOptions();
            Logger.LogInfo($"[SAIN] Custom preset '{name}' was deleted on the server.");

            if (isLoaded)
            {
                ReapplyLivePreset(null);
            }
            return;
        }

        string json = ServerPresetClient.GetCustom(name);
        if (string.IsNullOrEmpty(json) || json == "null")
        {
            return;
        }

        if (
            _customBundles.TryGetValue(name, out var existing)
            && string.Equals(JsonConvert.SerializeObject(existing, _settings), json, StringComparison.Ordinal)
        )
        {
            return;
        }

        var bundle = JsonConvert.DeserializeObject<SAINPresetBundle>(json, _settings);
        if (bundle?.Info == null)
        {
            return;
        }

        _customBundles[bundle.Info.Name] = bundle;
        PresetHandler.LoadCustomPresetOptions();
        Logger.LogInfo($"[SAIN] Custom preset '{name}' was updated on the server.");

        if (isLoaded)
        {
            ReapplyLivePreset(bundle.Info);
        }
    }

    public static void DeleteCustomPreset(string name)
    {
        _customBundles.Remove(name);
        try
        {
            ServerPresetClient.DeleteCustom(name);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to delete custom preset '{name}' from server: {ex.Message}");
        }
    }

    public static void PushCustomPreset(SAINPresetClass preset)
    {
        if (preset?.Info == null || !preset.Info.IsCustom)
        {
            return;
        }
        PublishCustom(preset, preset.Info);
    }

    public static void PublishCustom(SAINPresetClass preset, SAINPresetDefinition info)
    {
        if (!ServerConfigClient.EditingAllowed)
        {
            Logger.LogWarning("[SAIN] Preset editing is disabled by the server. Changes were not saved.");
            return;
        }

        try
        {
            var bundle = BuildBundle(preset);
            bundle.Info = info;
            _customBundles[info.Name] = bundle;

            string json = JsonConvert.SerializeObject(bundle, _settings);
            ServerPresetClient.SaveCustom(info.Name, json);
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to push custom preset to server: {ex.Message}");
        }
    }

    private static SAINPresetBundle BuildBundle(SAINPresetClass preset)
    {
        var bot = new Dictionary<SharedWildSpawnType, SAINSettingsGroupClass>();
        foreach (var kv in preset.BotSettings.SAINSettings)
        {
            bot[kv.Key.ToESain()] = kv.Value;
        }

        return new SAINPresetBundle
        {
            Info = preset.Info,
            GlobalSettings = preset.GlobalSettings,
            BotSettings = bot,
            Personalities = preset.PersonalityManager.PersonalityDictionary,
            GearStealthValues = preset.GearStealthValuesClass.ItemStealthValues,
        };
    }
}
