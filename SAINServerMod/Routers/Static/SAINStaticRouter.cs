using SAINServerMod.Callbacks;
using SAINServerMod.Models.Requests;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers + 5)]
public sealed class SAINStaticRouter(JsonUtil jsonUtil, SAINCallbacks callbacks)
    : StaticRouter(
        jsonUtil,
        [
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/config",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetClientConfig(url, info, sessionID)
            ),
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/namepersonalities",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetPersonalities(url, info, sessionID)
            ),
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/presets/defaults",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetDefaultPresets(url, info, sessionID)
            ),
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/bottypes/list",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetBotTypes(url, info, sessionID)
            ),
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/bottypes/exclusions",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetBotTypeExclusions(url, info, sessionID)
            ),
            new StreamedRouteAction<EmptyRequestData>(
                "/sain/presets/custom/list",
                async (url, info, sessionID, cancellationToken) => await callbacks.ListCustomPresets(url, info, sessionID)
            ),
            new StreamedRouteAction<PresetNameRequest>(
                "/sain/presets/custom/get",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetCustomPreset(url, info, sessionID)
            ),
            new RouteAction<PresetSaveRequest>(
                "/sain/presets/custom/save",
                async (url, info, sessionID, output, cancellationToken) => await callbacks.SaveCustomPreset(url, info, sessionID)
            ),
            new RouteAction<PresetNameRequest>(
                "/sain/presets/custom/delete",
                async (url, info, sessionID, output, cancellationToken) => await callbacks.DeleteCustomPreset(url, info, sessionID)
            ),
            new StreamedRouteAction<PresetNameRequest>(
                "/sain/data/get",
                async (url, info, sessionID, cancellationToken) => await callbacks.GetData(url, info, sessionID)
            ),
            new RouteAction<PresetSaveRequest>(
                "/sain/data/save",
                async (url, info, sessionID, output, cancellationToken) => await callbacks.SaveData(url, info, sessionID)
            ),
        ]
    ) { }
