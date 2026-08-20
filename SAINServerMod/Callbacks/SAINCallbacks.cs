using SAIN.Preset.Shared.Models.WS;
using SAINServerMod.Generation;
using SAINServerMod.Generators;
using SAINServerMod.Models.Requests;
using SAINServerMod.Models.Responses;
using SAINServerMod.Services;
using SAINServerMod.Utils;
using SAINServerMod.Ws;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Servers;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Callbacks;

[Injectable]
public sealed class SAINCallbacks(
    JsonUtil jsonUtil,
    ConfigService configService,
    PresetGenerationService presetGeneration,
    PresetService presetService,
    ClientDataStorageService clientData,
    SAINPresetWebSocketHandler presetSync,
    SainServerConfigService serverConfig
)
{
    public ValueTask<StreamedJsonBody> GetClientConfig(string url, EmptyRequestData info, string sessionID)
    {
        var config = serverConfig.Config;
        var response = new ClientConfigResponse
        {
            ForcedPresetName = config.ForcedPresetName,
            EditingAllowed = serverConfig.IsEditingAllowed(sessionID),
        };
        return new ValueTask<StreamedJsonBody>(
            SAINJsonUtil.StreamRaw(jsonUtil.Serialize(response) ?? throw new InvalidOperationException("Could not serialize client config!"))
        );
    }

    public ValueTask<StreamedJsonBody> GetPersonalities(string url, EmptyRequestData info, string sessionID)
    {
        return new ValueTask<StreamedJsonBody>(
            SAINJsonUtil.StreamRaw(
                jsonUtil.Serialize(configService.NicknamesModel) ?? throw new InvalidOperationException("Could not serialize personalities!")
            )
        );
    }

    public ValueTask<StreamedJsonBody> GetDefaultPresets(string url, EmptyRequestData info, string sessionID)
    {
        // Only individually-hidden presets are removed here, and a forced one always stays.
        var config = serverConfig.Config;
        var available = presetGeneration
            .Presets.Where(p => !config.DisabledPresets.Contains(p.Info.Name) || p.Info.Name == config.ForcedPresetName)
            .ToList();
        return new ValueTask<StreamedJsonBody>(SAINJsonUtil.StreamRaw(SAINJsonUtil.Serialize(available)));
    }

    public ValueTask<StreamedJsonBody> GetBotTypes(string url, EmptyRequestData info, string sessionID)
    {
        return new ValueTask<StreamedJsonBody>(SAINJsonUtil.StreamRaw(SAINJsonUtil.Serialize(DefaultBotTypes.Enabled())));
    }

    public ValueTask<StreamedJsonBody> GetBotTypeExclusions(string url, EmptyRequestData info, string sessionID)
    {
        return new ValueTask<StreamedJsonBody>(SAINJsonUtil.StreamRaw(SAINJsonUtil.Serialize(DefaultBotTypes.StrictExclusionList)));
    }

    public ValueTask<StreamedJsonBody> ListCustomPresets(string url, EmptyRequestData info, string sessionID)
    {
        return new ValueTask<StreamedJsonBody>(
            SAINJsonUtil.StreamRaw(
                jsonUtil.Serialize(presetService.ListCustom()) ?? throw new InvalidOperationException("Could not serialize custom preset list")
            )
        );
    }

    public async ValueTask<StreamedJsonBody> GetCustomPreset(string url, PresetNameRequest info, string sessionID)
    {
        return SAINJsonUtil.StreamRaw(await presetService.GetCustomAsync(info.Name) ?? "null");
    }

    public async ValueTask<string> SaveCustomPreset(string url, PresetSaveRequest info, string sessionID)
    {
        // Reject saves from players the admin hasn't authorised to edit in-game.
        if (!serverConfig.IsEditingAllowed(sessionID))
        {
            return jsonUtil.Serialize(new OkResponse { Ok = false })
                ?? throw new InvalidOperationException("Could not serialize custom preset save!");
        }

        await presetService.SaveCustomAsync(info.Name, info.PresetJson);
        await presetSync.BroadcastPresetChanged(info.Name, EPresetSyncChange.Saved, sessionID);
        return jsonUtil.Serialize(new OkResponse { Ok = true })
            ?? throw new InvalidOperationException("Could not serialize custom preset save!");
    }

    public async ValueTask<string> DeleteCustomPreset(string url, PresetNameRequest info, string sessionID)
    {
        bool deleted = presetService.DeleteCustom(info.Name);
        if (deleted)
        {
            if (presetService.ListCustom().Count == 0 && serverConfig.ClearDisabledDefaults())
            {
                await serverConfig.SaveAsync();
            }
            await presetSync.BroadcastPresetChanged(info.Name, EPresetSyncChange.Deleted, sessionID);
        }
        return jsonUtil.Serialize(new OkResponse { Ok = deleted })
            ?? throw new InvalidOperationException("Could not serialize custom preset delete!");
    }

    public async ValueTask<StreamedJsonBody> GetData(string url, PresetNameRequest info, string sessionID)
    {
        return SAINJsonUtil.StreamRaw(await clientData.GetAsync(info.Name) ?? "null");
    }

    public async ValueTask<string> SaveData(string url, PresetSaveRequest info, string sessionID)
    {
        await clientData.SaveAsync(info.Name, info.PresetJson);
        return jsonUtil.Serialize(new OkResponse { Ok = true }) ?? throw new InvalidOperationException("Could not serialize data save!");
    }
}
