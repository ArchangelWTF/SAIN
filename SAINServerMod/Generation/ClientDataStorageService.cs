using System.Reflection;
using SAINServerMod.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace SAINServerMod.Generation;

[Injectable(InjectionType.Singleton)]
public sealed class ClientDataStorageService(ModHelper modHelper, JsonFileStoreUtil jsonFileStore)
{
    private readonly string _root = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

    private static readonly Dictionary<string, string> _knownPaths = new(StringComparer.Ordinal)
    {
        ["CoreOverrides"] = "CoreOverrides.json",
    };

    public async Task<string?> GetAsync(string key)
    {
        return await jsonFileStore.ReadTextAsync(PathFor(key));
    }

    public async Task SaveAsync(string key, string json)
    {
        await jsonFileStore.WriteTextAsync(PathFor(key), json);
    }

    private string PathFor(string key)
    {
        string relative = _knownPaths.TryGetValue(key, out string? known)
            ? known
            : Path.Combine("ClientData", JsonFileStoreUtil.SanitizeFileName(key) + ".json");

        return Path.Combine(_root, relative);
    }
}
