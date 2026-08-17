using System.Reflection;
using SAINServerMod.Models.Preset.Personalities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Services;

[Injectable(InjectionType.Singleton)]
public sealed class ConfigService(ModHelper modHelper, JsonUtil jsonUtil)
{
    public string ModPath { get; init; } = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
    public NicknamesModel NicknamesModel { get; private set; } = default!;

    public async Task LoadAsync()
    {
        NicknamesModel nicknamesModel =
            await jsonUtil.DeserializeFromFileAsync<NicknamesModel>(Path.Combine(ModPath, "Data", "NicknamePersonalities.json"))
            ?? throw new InvalidOperationException("Could not load nicknames, is the mod installed correctly?");

        NicknamesModel = nicknamesModel;
    }
}
