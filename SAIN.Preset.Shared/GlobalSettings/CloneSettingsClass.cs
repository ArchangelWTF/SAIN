using System;
using System.Reflection;

namespace SAIN.Preset.Shared.GlobalSettings;

public static class CloneSettingsClass
{
    public static void CopyFields(object original, object clone)
    {
        Type type = original.GetType();
        FieldInfo[] fields = type.GetFields();

        foreach (FieldInfo field in fields)
        {
            var fieldType = field.FieldType;
            if (fieldType == type)
            {
                continue;
            }

            object originalValue = field.GetValue(original);
            field.SetValue(clone, originalValue);
        }
    }
}
