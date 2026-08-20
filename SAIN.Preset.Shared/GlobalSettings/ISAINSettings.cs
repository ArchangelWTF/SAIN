using System.Collections.Generic;

namespace SAIN.Preset.Shared.GlobalSettings;

public interface ISAINSettings
{
    void Update();
    object GetDefaults();
    void CreateDefault();
    void UpdateDefaults(object values);
    void Init(List<ISAINSettings> list);
}
