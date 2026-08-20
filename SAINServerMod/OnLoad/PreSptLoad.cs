using SAINServerMod.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace SAINServerMod.OnLoad;

[Injectable(TypePriority = OnLoadOrder.Preload + 5)]
public sealed class PreSptLoad(ConfigService configService) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await configService.LoadAsync();
    }
}
