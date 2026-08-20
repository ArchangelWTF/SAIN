using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings;

public abstract class SAINSettingsBase<T> : ISAINSettings
{
    public virtual void Update() { }

    public object GetDefaults()
    {
        return Defaults;
    }

    public void CreateDefault()
    {
        Defaults = (T)Activator.CreateInstance(typeof(T));
    }

    public void UpdateDefaults(object values)
    {
        CloneSettingsClass.CopyFields(values, Defaults);
    }

    [Hidden]
    [IgnoreDataMember]
    public T Defaults;

    public virtual void Init(List<ISAINSettings> list) { }
}
