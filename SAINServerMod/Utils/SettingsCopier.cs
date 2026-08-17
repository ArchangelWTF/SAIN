using System.Reflection;
using SAIN.Preset.Shared.Attributes;

namespace SAINServerMod.Utils;

public static class SettingsCopier
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;

    public static bool ShouldCopyDefault(FieldInfo field)
    {
        return field.GetCustomAttribute<CopyValueAttribute>() != null;
    }

    public static void CopyValues(object source, object target, Func<FieldInfo, bool>? shouldCopyField = null)
    {
        foreach (FieldInfo targetCategory in target.GetType().GetFields(Flags))
        {
            if (!TryGetMemberValue(source, targetCategory.Name, out object sourceCategory) || sourceCategory == null)
            {
                continue;
            }

            object targetCategoryObj = targetCategory.GetValue(target);

            foreach (FieldInfo targetVar in targetCategory.FieldType.GetFields(Flags))
            {
                if (shouldCopyField != null && !shouldCopyField(targetVar))
                {
                    continue;
                }
                if (!TryGetMemberValue(sourceCategory, targetVar.Name, out object value) || value == null)
                {
                    // Not present, or an unset nullable on the server: keep SAIN's default.
                    continue;
                }
                targetVar.SetValue(targetCategoryObj, value);
            }
        }
    }

    private static bool TryGetMemberValue(object obj, string name, out object value)
    {
        Type type = obj.GetType();

        FieldInfo field = type.GetField(name, Flags);
        if (field != null)
        {
            value = field.GetValue(obj);
            return true;
        }

        PropertyInfo property = type.GetProperty(name, Flags);
        if (property != null && property.CanRead)
        {
            value = property.GetValue(obj);
            return true;
        }

        value = null;
        return false;
    }
}
