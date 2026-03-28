using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Philiprehberger.ObjectDiff;

/// <summary>
/// Provides methods to compare two objects and return property-level changes.
/// </summary>
public static class ObjectDiff
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Compares two objects of the same type and returns a <see cref="DiffResult"/> containing all property-level changes.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="oldObj">The original object.</param>
    /// <param name="newObj">The updated object.</param>
    /// <param name="options">Optional comparison settings.</param>
    /// <returns>A <see cref="DiffResult"/> describing all detected changes.</returns>
    public static DiffResult Compare<T>(T? oldObj, T? newObj, DiffOptions? options = null)
    {
        options ??= new DiffOptions();
        var changes = new List<PropertyChange>();

        CompareInternal(typeof(T), oldObj, newObj, options, changes, prefix: "", depth: 0);

        return new DiffResult(changes.AsReadOnly());
    }

    /// <summary>
    /// Serializes a <see cref="DiffResult"/> to a JSON string.
    /// </summary>
    /// <param name="result">The diff result to serialize.</param>
    /// <returns>A JSON string representing the changes.</returns>
    public static string ToJson(DiffResult result)
    {
        var payload = new
        {
            changes = result.Changes.Select(c => new
            {
                property = c.PropertyName,
                old = c.OldValue,
                @new = c.NewValue
            }).ToArray()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static void CompareInternal(
        Type type,
        object? oldObj,
        object? newObj,
        DiffOptions options,
        List<PropertyChange> changes,
        string prefix,
        int depth)
    {
        if (ReferenceEquals(oldObj, newObj))
            return;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        if (oldObj is null && newObj is null)
            return;

        foreach (var prop in properties)
        {
            var fullName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (options.IgnoreProperties.Contains(fullName) || options.IgnoreProperties.Contains(prop.Name))
                continue;

            if (prop.GetCustomAttribute<DiffIgnoreAttribute>() is not null)
                continue;

            var oldValue = oldObj is not null ? prop.GetValue(oldObj) : null;
            var newValue = newObj is not null ? prop.GetValue(newObj) : null;

            if (IsDictionaryType(prop.PropertyType))
            {
                CompareDictionary(oldValue as IDictionary, newValue as IDictionary, fullName, changes);
            }
            else if (IsEnumerableType(prop.PropertyType))
            {
                CompareCollection(oldValue as IEnumerable, newValue as IEnumerable, fullName, changes);
            }
            else if (options.DeepCompare
                && depth < options.MaxDepth
                && IsComplexType(prop.PropertyType)
                && (oldValue is not null || newValue is not null))
            {
                CompareInternal(prop.PropertyType, oldValue, newValue, options, changes, fullName, depth + 1);
            }
            else
            {
                if (!Equals(oldValue, newValue))
                {
                    changes.Add(new PropertyChange(fullName, oldValue, newValue));
                }
            }
        }
    }

    /// <summary>
    /// Compares two collections element by element, detecting additions, removals, and modifications at each index.
    /// </summary>
    private static void CompareCollection(
        IEnumerable? oldCollection,
        IEnumerable? newCollection,
        string propertyName,
        List<PropertyChange> changes)
    {
        var oldList = ToObjectList(oldCollection);
        var newList = ToObjectList(newCollection);

        var maxCount = Math.Max(oldList.Count, newList.Count);

        for (var i = 0; i < maxCount; i++)
        {
            var oldValue = i < oldList.Count ? oldList[i] : null;
            var newValue = i < newList.Count ? newList[i] : null;

            if (!Equals(oldValue, newValue))
            {
                changes.Add(new PropertyChange($"{propertyName}[{i}]", oldValue, newValue));
            }
        }
    }

    /// <summary>
    /// Compares two dictionaries key by key, detecting additions, removals, and value changes.
    /// </summary>
    private static void CompareDictionary(
        IDictionary? oldDict,
        IDictionary? newDict,
        string propertyName,
        List<PropertyChange> changes)
    {
        var oldKeys = GetDictionaryKeys(oldDict);
        var newKeys = GetDictionaryKeys(newDict);

        var allKeys = new HashSet<object>(oldKeys);
        foreach (var key in newKeys)
            allKeys.Add(key);

        foreach (var key in allKeys)
        {
            var keyName = $"{propertyName}[{key}]";
            var oldValue = oldDict is not null && oldDict.Contains(key) ? oldDict[key] : null;
            var newValue = newDict is not null && newDict.Contains(key) ? newDict[key] : null;

            if (!Equals(oldValue, newValue))
            {
                changes.Add(new PropertyChange(keyName, oldValue, newValue));
            }
        }
    }

    private static List<object> GetDictionaryKeys(IDictionary? dict)
    {
        if (dict is null)
            return new List<object>();

        var keys = new List<object>();
        foreach (var key in dict.Keys)
            keys.Add(key);
        return keys;
    }

    private static List<object?> ToObjectList(IEnumerable? enumerable)
    {
        if (enumerable is null)
            return new List<object?>();

        var list = new List<object?>();
        foreach (var item in enumerable)
            list.Add(item);
        return list;
    }

    private static bool IsDictionaryType(Type type)
    {
        if (typeof(IDictionary).IsAssignableFrom(type))
            return true;

        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    private static bool IsEnumerableType(Type type)
    {
        if (type == typeof(string))
            return false;

        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive
            && !type.IsEnum
            && type != typeof(string)
            && type != typeof(decimal)
            && type != typeof(DateTime)
            && type != typeof(DateTimeOffset)
            && type != typeof(Guid)
            && !IsNullablePrimitive(type)
            && !IsDictionaryType(type)
            && !IsEnumerableType(type);
    }

    private static bool IsNullablePrimitive(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        return underlying is not null && (underlying.IsPrimitive || underlying.IsEnum || underlying == typeof(decimal));
    }
}
