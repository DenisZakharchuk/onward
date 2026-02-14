namespace Inventorization.Base.ADTs;

/// <summary>
/// Top-level ADT representing a complete search/query request.
/// Combines filtering, projection, sorting, and pagination.
/// </summary>
public sealed record SearchQuery
{
    /// <summary>
    /// Filter expression to apply (null means no filtering)
    /// </summary>
    public FilterExpression? Filter { get; init; }
    
    /// <summary>
    /// Field projection specification (null means all fields)
    /// </summary>
    public ProjectionRequest? Projection { get; init; }
    
    /// <summary>
    /// Sort specification (null means no specific ordering)
    /// </summary>
    public SortRequest? Sort { get; init; }
    
    /// <summary>
    /// Pagination parameters (required)
    /// </summary>
    public PageRequest Pagination { get; init; }
    
    /// <summary>
    /// Parameterless constructor for deserialization
    /// </summary>
    public SearchQuery()
    {
        Pagination = new PageRequest();
    }
    
    /// <summary>
    /// Constructs a search query with all parameters
    /// </summary>
    public SearchQuery(
        FilterExpression? filter = null,
        ProjectionRequest? projection = null,
        SortRequest? sort = null,
        PageRequest? pagination = null)
    {
        Filter = filter;
        Projection = projection;
        Sort = sort;
        Pagination = pagination ?? new PageRequest();
    }
}

/// <summary>
/// Pagination parameters for search queries
/// </summary>
public sealed record PageRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; init; } = 1;
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; } = 10;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public PageRequest() { }
    
    /// <summary>
    /// Constructs pagination with specific page and size
    /// </summary>
    public PageRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
