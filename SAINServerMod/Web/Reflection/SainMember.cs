using System.Reflection;

namespace SAINServerMod.Web.Reflection;

public sealed class SainMember
{
    private readonly FieldInfo? _field;
    private readonly PropertyInfo? _property;

    public object Owner { get; }
    public MemberInfo Member { get; }
    public SainMeta Meta { get; }
    public Type ValueType { get; }
    public SainFieldKind Kind { get; }

    public SainMember(object owner, MemberInfo member)
    {
        Owner = owner;
        Member = member;
        Meta = SainMeta.Read(member);

        _field = member as FieldInfo;
        _property = member as PropertyInfo;
        ValueType = _field?.FieldType ?? _property!.PropertyType;
        Kind = SainValue.Classify(ValueType);
    }

    /// <summary>Uniform accessor used by the field editor, carrying reset-to-default support.</summary>
    public SainValue ToValue()
    {
        return new SainValue
        {
            Meta = Meta,
            ValueType = ValueType,
            Getter = GetValue,
            Setter = SetValue,
            CanWrite = CanWrite,
            DefaultProvider = DefaultValue,
        };
    }

    public object? GetValue()
    {
        return _field != null ? _field.GetValue(Owner) : _property!.GetValue(Owner);
    }

    public void SetValue(object? value)
    {
        if (_field != null)
        {
            _field.SetValue(Owner, value);
        }
        else if (_property!.CanWrite)
        {
            _property.SetValue(Owner, value);
        }
    }

    public bool CanWrite
    {
        get { return _field != null || (_property?.CanWrite ?? false); }
    }

    /// <summary>
    /// The value this member has on a freshly constructed instance of the owner type, matching the
    /// client's "reset to default" behavior. Falls back to the DefaultFloat attribute when present.
    /// </summary>
    public object? DefaultValue()
    {
        if (Meta.HasDefaultFloat && (Kind == SainFieldKind.Float || Kind == SainFieldKind.Int))
        {
            return Kind == SainFieldKind.Int ? (int)Meta.DefaultFloat : Meta.DefaultFloat;
        }

        var fresh = DefaultInstanceCache.Get(Owner.GetType());
        if (fresh == null)
        {
            return null;
        }
        return _field != null ? _field.GetValue(fresh) : _property!.GetValue(fresh);
    }

    private static class DefaultInstanceCache
    {
        private static readonly Dictionary<Type, object?> _cache = new();

        public static object? Get(Type type)
        {
            if (_cache.TryGetValue(type, out var value))
            {
                return value;
            }
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch { }
            _cache[type] = instance;
            return instance;
        }
    }
}
