using System.Text.Json.Serialization;
using Inventorization.Base.ADTs.Converters;

namespace Inventorization.Base.ADTs;

/// <summary>
/// Specifies sorting order for query results.
/// Supports multi-field sorting with ascending/descending direction.
/// </summary>
[JsonConverter(typeof(SortRequestConverter))]
public sealed record SortRequest(IReadOnlyList<SortField> Fields)
{
    /// <summary>
    /// Convenience constructor for params-style initialization
    /// </summary>
    public SortRequest(params SortField[] fields) : this((IReadOnlyList<SortField>)fields) { }
    
    /// <summary>
    /// Creates a sort request with no specific ordering (database default)
    /// </summary>
    public static SortRequest Default() => new(Array.Empty<SortField>());
}

/// <summary>
/// Represents a single field to sort by with direction
/// </summary>
public sealed record SortField(string FieldName, SortDirection Direction)
{
    /// <summary>
    /// Creates an ascending sort field
    /// </summary>
    public static SortField Ascending(string fieldName) => new(fieldName, SortDirection.Ascending);
    
    /// <summary>
    /// Creates a descending sort field
    /// </summary>
    public static SortField Descending(string fieldName) => new(fieldName, SortDirection.Descending);
}

/// <summary>
/// Sort direction enumeration
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending order (A-Z, 0-9, oldest to newest)
    /// </summary>
    Ascending,
    
    /// <summary>
    /// Descending order (Z-A, 9-0, newest to oldest)
    /// </summary>
    Descending
}
