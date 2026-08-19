using System.Reflection;
using SAIN.Preset.Shared;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GearStealthValues;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;
using SAIN.Preset.Shared.Preset;
using SAINServerMod.Models.Preset;
using SAINServerMod.Utils;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace SAINServerMod.Services;

[Injectable(InjectionType.Singleton)]
public sealed class PresetService(ModHelper modHelper, JsonFileStoreUtil jsonFileStore, ISptLogger<PresetService> logger)
{
    public event Action? CustomPresetsChanged;

    private readonly string _root = Path.Combine(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "Presets");

    private async Task<GeneratedPreset?> TryReadOneAsync(string dir)
    {
        string presetName = Path.GetFileName(dir);
        var diff = new PresetSchemaDiff(presetName);

        var info = await ReadEntryAsync<SAINPresetDefinition>(Path.Combine(dir, "Info.json"), diff, "Info");

        if (info == null)
        {
            logger.Warning($"[SAIN] Preset folder '{presetName}' has no readable Info.json.");
            return null;
        }

        var global = await ReadEntryAsync<GlobalSettingsClass>(Path.Combine(dir, "GlobalSettings.json"), diff, "GlobalSettings");

        if (global == null)
        {
            logger.Warning($"[SAIN] Preset folder '{presetName}' has no readable GlobalSettings.json.");
            return null;
        }

        var botSettings = new Dictionary<ESainWildSpawnType, SAINSettingsGroupClass>();
        string botDir = Path.Combine(dir, "BotSettings");
        if (Directory.Exists(botDir))
        {
            foreach (string file in Directory.GetFiles(botDir, "*.json"))
            {
                var group = await ReadEntryAsync<SAINSettingsGroupClass>(file, diff, "BotSettings[*]");
                if (group != null)
                {
                    botSettings[group.WildSpawnType] = group;
                }
            }
        }

        if (botSettings.Count == 0)
        {
            logger.Warning($"[SAIN] Preset folder '{presetName}' has no readable bot settings.");
            return null;
        }

        var personalities = new Dictionary<EPersonality, PersonalitySettingsClass>();
        string persDir = Path.Combine(dir, "Personalities");
        if (Directory.Exists(persDir))
        {
            foreach (string file in Directory.GetFiles(persDir, "*.json"))
            {
                if (!Enum.TryParse(Path.GetFileNameWithoutExtension(file), out EPersonality key))
                {
                    diff.SkipFile(file);
                    continue;
                }
                var settings = await ReadEntryAsync<PersonalitySettingsClass>(file, diff, "Personalities[*]");
                if (settings != null)
                {
                    personalities[key] = settings;
                }
            }
        }

        var stealth = new Dictionary<EEquipmentType, List<ItemStealthValue>>();
        string stealthDir = Path.Combine(dir, "ItemStealthValues");
        if (Directory.Exists(stealthDir))
        {
            foreach (string file in Directory.GetFiles(stealthDir, "*.json"))
            {
                var item = await ReadEntryAsync<ItemStealthValue>(file, diff, "GearStealthValues[*][*]");
                if (item == null)
                {
                    continue;
                }
                if (!stealth.TryGetValue(item.EquipmentType, out var list))
                {
                    list = [];
                    stealth[item.EquipmentType] = list;
                }
                list.Add(item);
            }
        }

        Report(diff);

        return new GeneratedPreset(info, global, botSettings, personalities, stealth);
    }

    private void Report(PresetSchemaDiff diff)
    {
        if (diff.SkippedFiles.Count > 0)
        {
            logger.Warning(
                $"[SAIN] Skipped {diff.SkippedFiles.Count} file(s) in preset '{diff.Name}' naming a bot type or personality this SAIN version does not have: {Describe(diff.SkippedFiles)}"
            );
        }

        if (diff.Added.Count > 0)
        {
            logger.Warning(
                $"[SAIN] Preset '{diff.Name}' predates {diff.Added.Count} setting(s), filled in at their default values: {Describe(diff.Added)}"
            );
        }

        if (diff.Removed.Count > 0)
        {
            logger.Warning(
                $"[SAIN] Preset '{diff.Name}' has {diff.Removed.Count} setting(s) this version does not save, left out: {Describe(diff.Removed)}"
            );
        }
    }

    private static string Describe(IReadOnlyCollection<string> paths)
    {
        const int maxListed = 12;

        string listed = string.Join(", ", paths.Take(maxListed));
        return paths.Count > maxListed ? $"{listed}, and {paths.Count - maxListed} more" : listed;
    }

    private async Task<T?> ReadEntryAsync<T>(string file, PresetSchemaDiff diff, string schemaPath)
        where T : class
    {
        try
        {
            string? text = await jsonFileStore.ReadTextAsync(file);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var value = SAINJsonUtil.Deserialize<T>(text);
            if (value == null)
            {
                return null;
            }

            PresetUpgrader.DiffInto(diff, schemaPath, text, SAINJsonUtil.Serialize(value));
            return value;
        }
        catch (Exception)
        {
            diff.SkipFile(file);
            return null;
        }
    }

    public List<string> ListCustom()
    {
        Directory.CreateDirectory(_root);
        return Directory.GetFiles(_root, "*.json").Select(Path.GetFileNameWithoutExtension).ToList()!;
    }

    public async Task<string?> GetCustomAsync(string name)
    {
        return await jsonFileStore.ReadTextAsync(CustomPathFor(name));
    }

    public async Task SaveCustomAsync(string name, string presetJson)
    {
        await jsonFileStore.WriteTextAsync(CustomPathFor(name), presetJson);
        CustomPresetsChanged?.Invoke();
    }

    public bool DeleteCustom(string name)
    {
        string path = CustomPathFor(name);
        if (!File.Exists(path))
        {
            return false;
        }
        File.Delete(path);
        CustomPresetsChanged?.Invoke();
        return true;
    }

    public async Task<List<string>> ImportCustomDirectoriesAsync()
    {
        var imported = new List<string>();
        Directory.CreateDirectory(_root);

        var existing = new HashSet<string>(ListCustom(), StringComparer.OrdinalIgnoreCase);

        foreach (string dir in Directory.GetDirectories(_root))
        {
            var info = await ReadEntryAsync<SAINPresetDefinition>(
                Path.Combine(dir, "Info.json"),
                new PresetSchemaDiff(Path.GetFileName(dir)),
                "Info"
            );
            if (info == null)
            {
                continue;
            }

            if (info.IsCustom && !existing.Contains(info.Name))
            {
                GeneratedPreset? preset;
                try
                {
                    preset = await TryReadOneAsync(dir);
                }
                catch (Exception ex)
                {
                    logger.Error($"[SAIN] Could not import preset folder '{Path.GetFileName(dir)}' {ex.Message}");
                    continue;
                }

                if (preset == null)
                {
                    continue;
                }

                await SaveCustomAsync(info.Name, SAINJsonUtil.Serialize(preset.ToBundle()));
                imported.Add(info.Name);
            }

            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch { }
        }

        return imported;
    }

    public async Task<List<PresetSchemaDiff>> UpgradeCustomToCurrentSchemaAsync()
    {
        var upgraded = new List<PresetSchemaDiff>();

        foreach (string name in ListCustom())
        {
            string? saved = await GetCustomAsync(name);

            if (string.IsNullOrWhiteSpace(saved))
            {
                continue;
            }

            string current;
            PresetSchemaDiff diff;
            try
            {
                var bundle = SAINJsonUtil.Deserialize<SAINPresetBundle>(saved);
                if (bundle == null)
                {
                    continue;
                }
                current = SAINJsonUtil.Serialize(bundle);
                diff = PresetUpgrader.Diff(name, saved, current);
            }
            catch
            {
                continue;
            }

            if (!diff.HasChanges)
            {
                continue;
            }

            await jsonFileStore.WriteTextAsync(CustomPathFor(name), current);
            Report(diff);
            upgraded.Add(diff);
        }

        if (upgraded.Count > 0)
        {
            CustomPresetsChanged?.Invoke();
        }

        return upgraded;
    }

    private string CustomPathFor(string name)
    {
        return Path.Combine(_root, JsonFileStoreUtil.SanitizeFileName(name) + ".json");
    }
}
