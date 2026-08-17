using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SAINServerMod.Utils;

public static class SAINJsonUtil
{
    public static readonly JsonSerializerOptions Indented = new()
    {
        IncludeFields = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { DataContractTypeInfoResolver } },
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, Indented);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, Indented);
    }

    private static void DataContractTypeInfoResolver(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        if (typeInfo.Type.GetCustomAttribute<DataContractAttribute>() is null)
        {
            return;
        }

        foreach (JsonPropertyInfo property in typeInfo.Properties.ToList())
        {
            var member = property.AttributeProvider as MemberInfo;

            if (member?.GetCustomAttribute<DataMemberAttribute>() is null)
            {
                typeInfo.Properties.Remove(property);
            }
        }
    }
}
