using SAIN.Preset.Shared;
using SAINServerMod.Generation;
using SAINServerMod.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace SAINServerMod.OnLoad;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 5)]
public sealed class PostDbLoad(ClientDataStorageService clientData) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken = default)
    {
        await GenerateCoreOverrides();
    }

    private async Task GenerateCoreOverrides()
    {
        string key = nameof(CoreOverrides);

        if (await clientData.GetAsync(key) != null)
        {
            return;
        }

        await clientData.SaveAsync(key, SAINJsonUtil.Serialize(new CoreOverrides()));
    }
}
