using System.Text.Json.Serialization;

namespace SAINServerMod.Models.Responses;

public sealed record OkResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }
}
