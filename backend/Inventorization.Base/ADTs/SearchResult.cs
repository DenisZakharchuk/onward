namespace Inventorization.Base.ADTs;

/// <summary>
/// Generic result wrapper for search query responses.
/// Contains paginated results with metadata.
/// </summary>
public sealed record SearchResult<TProjection>
{
    /// <summary>
    /// List of projected items matching the query
    /// </summary>
    public IReadOnlyList<TProjection> Items { get; init; } = Array.Empty<TProjection>();
    
    /// <summary>
    /// Total count of items matching the filter (across all pages)
    /// </summary>
    public int TotalCount { get; init; }
    
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (TotalCount + PageSize - 1) / PageSize : 0;
    
    /// <summary>
    /// Whether there are more pages available
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// Whether there are previous pages available
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}
