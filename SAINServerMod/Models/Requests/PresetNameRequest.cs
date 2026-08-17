using SPTarkov.Server.Core.Models.Utils;

namespace SAINServerMod.Models.Requests;

public sealed record PresetNameRequest : IRequestData
{
    public string Name { get; set; } = string.Empty;
}
