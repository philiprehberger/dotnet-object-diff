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
}
