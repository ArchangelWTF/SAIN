using System.Threading;
using System.Threading.Tasks;

namespace SAIN.ServerInterop;

public interface ISainBotTypeRegistry
{
    Task RegisterAsync(SainBotTypeRegistration registration, CancellationToken cancellationToken = default);
}
