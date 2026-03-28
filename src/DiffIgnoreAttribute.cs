namespace Philiprehberger.ObjectDiff;

/// <summary>
/// Marks a property to be excluded from object comparison.
/// Properties decorated with this attribute will be skipped by <see cref="ObjectDiff.Compare{T}"/>,
/// providing an alternative to string-based <see cref="DiffOptions.IgnoreProperties"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DiffIgnoreAttribute : Attribute
{
}
