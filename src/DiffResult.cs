namespace Philiprehberger.ObjectDiff;

/// <summary>
/// Contains the result of comparing two objects, including all detected property changes.
/// </summary>
/// <param name="Changes">The list of property-level changes detected between the two objects.</param>
public record DiffResult(IReadOnlyList<PropertyChange> Changes)
{
    /// <summary>
    /// Gets a value indicating whether any property changes were detected.
    /// </summary>
    public bool HasChanges => Changes.Count > 0;

    /// <summary>
    /// Returns a human-readable summary of all detected changes.
    /// Each entry describes a single property change in plain English,
    /// e.g., "Name changed from 'Alice' to 'Bob'".
    /// </summary>
    /// <returns>A read-only list of human-readable change descriptions.</returns>
    public IReadOnlyList<string> GetSummary()
    {
        var summaries = new List<string>(Changes.Count);

        foreach (var change in Changes)
        {
            var oldDisplay = FormatValue(change.OldValue);
            var newDisplay = FormatValue(change.NewValue);
            summaries.Add($"{change.PropertyName} changed from {oldDisplay} to {newDisplay}");
        }

        return summaries.AsReadOnly();
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return "null";

        return $"'{value}'";
    }
}
