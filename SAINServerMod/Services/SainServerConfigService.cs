using System.Reflection;
using SAINServerMod.Models;
using SAINServerMod.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;

namespace SAINServerMod.Services;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PostLoad + 5)]
public sealed class SainServerConfigService(ModHelper modHelper, JsonFileStoreUtil jsonFileStore) : IOnLoad
{
    private readonly string _path = Path.Combine(
        modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()),
        "ServerConfig.json"
    );

    public SAINServerConfig Config { get; private set; } = new();

    public async Task OnLoadAsync(CancellationToken cancellationToken = default)
    {
        Config = await jsonFileStore.ReadAsync<SAINServerConfig>(_path) ?? new SAINServerConfig();
    }

    public async Task SaveAsync()
    {
        await jsonFileStore.WriteAsync(_path, Config);
    }

    /// <summary>A session may save presets when editing is open to all, or the session is allowlisted.</summary>
    public bool IsEditingAllowed(string sessionId)
    {
        return Config.AllowClientEditing || Config.EditAllowlist.Contains(sessionId);
    }

    /// <summary>Re-enables every hidden default. Returns true if anything changed.</summary>
    public bool ClearDisabledDefaults()
    {
        if (Config.DisabledPresets.Count == 0)
        {
            return false;
        }
        Config.DisabledPresets.Clear();
        return true;
    }
}
