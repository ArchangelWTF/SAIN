using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities;

public abstract class SettingsGroupBase<T> : ISettingsGroup
{
    [IgnoreDataMember]
    [Hidden]
    public List<ISAINSettings> SettingsList { get; } = new List<ISAINSettings>();

    public virtual void InitList()
    {
        if (!initialized)
        {
            initialized = true;
        }
    }

    public virtual void Init()
    {
        InitList();
        CreateDefaults();
        Update();
    }

    public void Update()
    {
        foreach (var item in SettingsList)
        {
            item.Update();
        }
    }

    protected bool initialized;

    public void CreateDefaults()
    {
        foreach (var item in SettingsList)
        {
            item.CreateDefault();
        }
    }

    public void UpdateDefaults(ISettingsGroup replacementGroup = null)
    {
        if (replacementGroup == null)
        {
            foreach (var item in SettingsList)
            {
                item.UpdateDefaults(item);
            }
            return;
        }

        replacementGroup.InitList();
        for (int i = 0; i < SettingsList.Count; i++)
        {
            var item = SettingsList[i];
            var replacement = replacementGroup.SettingsList[i];
            item.UpdateDefaults(replacement);
        }
    }
}
