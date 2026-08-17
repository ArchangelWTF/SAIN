using System.Collections;

namespace SAINServerMod.Web.Reflection;

public sealed class SainValue
{
    public required SainMeta Meta { get; init; }
    public required Type ValueType { get; init; }
    public required Func<object?> Getter { get; init; }
    public required Action<object?> Setter { get; init; }
    public Func<object?>? DefaultProvider { get; init; }
    public bool CanWrite { get; init; } = true;

    public SainFieldKind Kind
    {
        get { return Classify(ValueType); }
    }

    public object? Get()
    {
        return Getter();
    }

    public void Set(object? value)
    {
        Setter(value);
    }

    public bool HasDefault
    {
        get { return DefaultProvider != null; }
    }

    public object? Default()
    {
        return DefaultProvider?.Invoke();
    }

    public static SainFieldKind Classify(Type type)
    {
        if (type == typeof(bool))
        {
            return SainFieldKind.Bool;
        }

        if (type == typeof(float) || type == typeof(double))
        {
            return SainFieldKind.Float;
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
        {
            return SainFieldKind.Int;
        }

        if (type == typeof(string))
        {
            return SainFieldKind.String;
        }

        if (type.IsEnum)
        {
            return SainFieldKind.Enum;
        }

        if (typeof(IDictionary).IsAssignableFrom(type) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
        {
            return SainFieldKind.Dictionary;
        }

        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
        {
            return SainFieldKind.List;
        }

        if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
        {
            return SainFieldKind.Nested;
        }

        return SainFieldKind.Unsupported;
    }
}
