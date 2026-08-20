using SAIN.Preset.Shared.Models.WS;
using SAIN.Preset.Shared.Preset;
using SAINServerMod.Generators;
using SAINServerMod.Models.Preset;
using SAINServerMod.Services;
using SAINServerMod.Utils;
using SAINServerMod.Ws;
using SPTarkov.DI.Annotations;

namespace SAINServerMod.Web.Services;

public sealed record PresetListItem(string Name, string? Description, bool IsCustom, string? BaseDifficulty, bool Disabled = false);

/// <summary>A preset opened for editing, plus whether saving it will create a brand-new custom preset.</summary>
public sealed record OpenedPreset(SAINPresetBundle Bundle, bool IsNewCustom, string? OriginalName);

[Injectable(InjectionType.Singleton)]
public sealed class SainPresetEditService(
    PresetService presetService,
    PresetGenerationService presetGeneration,
    SAINPresetWebSocketHandler webSocket,
    SainServerConfigService serverConfig
)
{
    public IReadOnlyList<PresetListItem> ListAll()
    {
        var items = new List<PresetListItem>();

        foreach (var preset in presetGeneration.Presets.OrderBy(p => (int)p.Info.BaseSAINDifficulty))
        {
            items.Add(
                new PresetListItem(
                    preset.Info.Name,
                    preset.Info.Description,
                    IsCustom: false,
                    preset.Info.BaseSAINDifficulty.ToString(),
                    Disabled: serverConfig.Config.DisabledPresets.Contains(preset.Info.Name)
                )
            );
        }

        foreach (var name in presetService.ListCustom())
        {
            items.Add(new PresetListItem(name, Description: null, IsCustom: true, BaseDifficulty: null));
        }

        return items;
    }

    public bool CustomExists(string name)
    {
        return presetService.ListCustom().Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Opens a preset for editing. Custom presets load as-is, defaults are cloned into an editable copy.</summary>
    public async Task<OpenedPreset?> OpenAsync(string name)
    {
        var customJson = await presetService.GetCustomAsync(name);
        if (!string.IsNullOrWhiteSpace(customJson))
        {
            var bundle = SAINJsonUtil.Deserialize<SAINPresetBundle>(customJson);
            if (bundle != null)
            {
                bundle.Info ??= new SAINPresetDefinition { Name = name };
                bundle.Info.IsCustom = true;
                bundle.Info.CanEditName = true;
                return new OpenedPreset(bundle, IsNewCustom: false, OriginalName: name);
            }
        }

        var generated = presetGeneration.Presets.FirstOrDefault(p => string.Equals(p.Info.Name, name, StringComparison.OrdinalIgnoreCase));
        if (generated == null)
        {
            return null;
        }

        var deep = SAINJsonUtil.Deserialize<SAINPresetBundle>(SAINJsonUtil.Serialize(generated.ToBundle()))!;
        deep.Info ??= new SAINPresetDefinition { Name = generated.Info.Name };
        deep.Info.CanEditName = true;
        return new OpenedPreset(deep, IsNewCustom: true, OriginalName: generated.Info.Name);
    }

    public async Task SaveAsync(SAINPresetBundle bundle, string? renamedFrom = null)
    {
        bundle.Info ??= new SAINPresetDefinition();
        bundle.Info.IsCustom = true;
        bundle.Info.CanEditName = true;

        // If the preset was renamed, drop the old custom file so we don't leave a stale duplicate.
        if (
            !string.IsNullOrWhiteSpace(renamedFrom)
            && !string.Equals(renamedFrom, bundle.Info.Name, StringComparison.Ordinal)
            && CustomExists(renamedFrom)
        )
        {
            presetService.DeleteCustom(renamedFrom);
        }

        var json = SAINJsonUtil.Serialize(bundle);
        await presetService.SaveCustomAsync(bundle.Info.Name, json);
        await webSocket.BroadcastPresetChanged(bundle.Info.Name, EPresetSyncChange.Saved);
    }

    public async Task<bool> DeleteAsync(string name)
    {
        var deleted = presetService.DeleteCustom(name);
        if (deleted)
        {
            // Deleting the last custom re-enables all defaults so players are never left with nothing.
            if (presetService.ListCustom().Count == 0 && serverConfig.ClearDisabledDefaults())
            {
                await serverConfig.SaveAsync();
            }
            await webSocket.BroadcastPresetChanged(name, EPresetSyncChange.Deleted);
        }
        return deleted;
    }

    public async Task<string?> DuplicateAsync(string sourceName, string newName)
    {
        var opened = await OpenAsync(sourceName);
        if (opened == null)
        {
            return null;
        }

        opened.Bundle.Info.Name = newName;
        opened.Bundle.Info.CanEditName = true;
        opened.Bundle.Info.IsCustom = true;
        await SaveAsync(opened.Bundle);
        return newName;
    }
}
