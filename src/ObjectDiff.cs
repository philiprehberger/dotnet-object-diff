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

            var oldValue = oldObj is not null ? prop.GetValue(oldObj) : null;
            var newValue = newObj is not null ? prop.GetValue(newObj) : null;

            if (options.DeepCompare
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

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive
            && !type.IsEnum
            && type != typeof(string)
            && type != typeof(decimal)
            && type != typeof(DateTime)
            && type != typeof(DateTimeOffset)
            && type != typeof(Guid)
            && !IsNullablePrimitive(type);
    }

    private static bool IsNullablePrimitive(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        return underlying is not null && (underlying.IsPrimitive || underlying.IsEnum || underlying == typeof(decimal));
    }
}
