namespace Philiprehberger.ObjectDiff;

/// <summary>
/// Represents a single property-level change between two objects.
/// </summary>
/// <param name="PropertyName">The name of the changed property. Uses dot-notation for nested properties (e.g., "Address.City").</param>
/// <param name="OldValue">The previous value of the property.</param>
/// <param name="NewValue">The new value of the property.</param>
public record PropertyChange(string PropertyName, object? OldValue, object? NewValue);
