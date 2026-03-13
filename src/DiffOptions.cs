namespace Philiprehberger.ObjectDiff;

/// <summary>
/// Configuration options for object comparison.
/// </summary>
public class DiffOptions
{
    /// <summary>
    /// Gets the set of property names to exclude from comparison.
    /// Property names are compared case-insensitively.
    /// </summary>
    public ISet<string> IgnoreProperties { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets a value indicating whether to recursively compare nested complex objects.
    /// When enabled, nested properties use dot-notation (e.g., "Address.City").
    /// Default is <c>false</c>.
    /// </summary>
    public bool DeepCompare { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum recursion depth for deep comparison.
    /// Only applies when <see cref="DeepCompare"/> is enabled.
    /// Default is 3.
    /// </summary>
    public int MaxDepth { get; set; } = 3;
}
