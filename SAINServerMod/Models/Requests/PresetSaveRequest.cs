using SPTarkov.Server.Core.Models.Utils;

namespace SAINServerMod.Models.Requests;

public sealed record PresetSaveRequest : IRequestData
{
    public string Name { get; set; } = string.Empty;
    public string PresetJson { get; set; } = string.Empty;
}
