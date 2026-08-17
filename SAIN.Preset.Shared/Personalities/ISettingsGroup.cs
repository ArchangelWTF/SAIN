using System.Collections.Generic;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities;

public interface ISettingsGroup
{
    void Init();
    void Update();
    List<ISAINSettings> SettingsList { get; }
    void InitList();
    void CreateDefaults();
    void UpdateDefaults(ISettingsGroup replacementValues = null);
}
