using SAINServerMod.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace SAINServerMod.OnLoad;

// SPT 4.1: OnLoadOrder.PreSptModLoader is gone -- Preload is now the earliest
// stage -- and IOnLoad.OnLoad() became OnLoadAsync(CancellationToken).
[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public sealed class PreSptLoad(ConfigService configService) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await configService.LoadAsync();
    }
}
