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
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace SAINServerMod.Services;

[Injectable(InjectionType.Singleton)]
public sealed class PresetService(ModHelper modHelper, JsonFileStoreUtil jsonFileStore)
{
    public event Action? CustomPresetsChanged;

    private readonly string _root = Path.Combine(modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "Presets");

    private async Task<GeneratedPreset?> TryReadOneAsync(string dir)
    {
        var info = await jsonFileStore.ReadAsync<SAINPresetDefinition>(Path.Combine(dir, "Info.json"));

        if (info == null)
        {
            return null;
        }

        var global = await jsonFileStore.ReadAsync<GlobalSettingsClass>(Path.Combine(dir, "GlobalSettings.json"));

        if (global == null)
        {
            return null;
        }

        var botSettings = new Dictionary<ESainWildSpawnType, SAINSettingsGroupClass>();
        string botDir = Path.Combine(dir, "BotSettings");
        if (Directory.Exists(botDir))
        {
            foreach (string file in Directory.GetFiles(botDir, "*.json"))
            {
                var group = await jsonFileStore.ReadAsync<SAINSettingsGroupClass>(file);
                if (group != null)
                {
                    botSettings[group.WildSpawnType] = group;
                }
            }
        }

        if (botSettings.Count == 0)
        {
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
                    continue;
                }
                var settings = await jsonFileStore.ReadAsync<PersonalitySettingsClass>(file);
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
                var item = await jsonFileStore.ReadAsync<ItemStealthValue>(file);
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

        return new GeneratedPreset(info, global, botSettings, personalities, stealth);
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
            var info = await jsonFileStore.ReadAsync<SAINPresetDefinition>(Path.Combine(dir, "Info.json"));
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
                catch
                {
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

    private string CustomPathFor(string name)
    {
        return Path.Combine(_root, JsonFileStoreUtil.SanitizeFileName(name) + ".json");
    }
}
