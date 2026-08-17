namespace SAINServerMod.Models;

public sealed class SAINServerConfig
{
    public List<string> DisabledPresets { get; set; } = [];
    public string? ForcedPresetName { get; set; }
    public bool AllowClientEditing { get; set; } = true;
    public List<string> EditAllowlist { get; set; } = [];
}
