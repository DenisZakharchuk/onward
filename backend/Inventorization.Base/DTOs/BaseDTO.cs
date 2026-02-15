namespace Inventorization.Base.DTOs;

/// <summary>
/// Base class for all DTOs
/// </summary>
public abstract class BaseDTO
{
    public Guid Id { get; set; }
}

/// <summary>
/// Base class for Create DTOs
/// </summary>
public abstract class CreateDTO
{
}

/// <summary>
/// Base class for Update DTOs
/// </summary>
public abstract class UpdateDTO : BaseDTO
{
}

/// <summary>
/// Base class for Delete DTOs
/// </summary>
public abstract class DeleteDTO : BaseDTO
{
}

/// <summary>
/// Base record for init operations
/// </summary>
public record InitDTO(Guid Id)
{
}

/// <summary>
/// Base class for Details DTOs (returned by GetByIdAsync)
/// </summary>
public abstract class DetailsDTO : BaseDTO
{
}

/// <summary>
/// Base class for Search/List DTOs
/// </summary>
public abstract class SearchDTO
{
    public PageDTO? Page { get; set; }
}

/// <summary>
/// Pagination information
/// </summary>
public class PageDTO
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Generic result wrapper for service responses
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult<T> Success(T data, string? message = null) =>
        new() { IsSuccess = true, Data = data, Message = message };

    public static ServiceResult<T> Failure(string message, List<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors ?? new() };

    public static ServiceResult<T> Failure(List<string> errors) =>
        new() { IsSuccess = false, Errors = errors };
}

/// <summary>
/// Paged result wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>
/// Represents changes to entity relationships with explicit add/remove semantics
/// </summary>
public class EntityReferencesDTO
{
    /// <summary>
    /// IDs of entities to add to the relationship
    /// </summary>
    public List<Guid> IdsToAdd { get; set; } = new();
    
    /// <summary>
    /// IDs of entities to remove from the relationship
    /// </summary>
    public List<Guid> IdsToRemove { get; set; } = new();
    
    public EntityReferencesDTO() { }
    
    public EntityReferencesDTO(IEnumerable<Guid> idsToAdd, IEnumerable<Guid>? idsToRemove = null)
    {
        IdsToAdd = idsToAdd.ToList();
        IdsToRemove = idsToRemove?.ToList() ?? new();
    }
    
    /// <summary>
    /// Returns true if there are any changes to apply
    /// </summary>
    public bool HasChanges => IdsToAdd.Any() || IdsToRemove.Any();
}

/// <summary>
/// Result of a relationship update operation
/// </summary>
public class RelationshipUpdateResult
{
    public bool IsSuccess { get; set; }
    public int AddedCount { get; set; }
    public int RemovedCount { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    
    public static RelationshipUpdateResult Success(int added, int removed, string? message = null) =>
        new() 
        { 
            IsSuccess = true, 
            AddedCount = added, 
            RemovedCount = removed, 
            Message = message ?? $"Added {added}, removed {removed} relationships" 
        };
    
    public static RelationshipUpdateResult Failure(string message, List<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors ?? new() };
}

/// <summary>
/// Result of bulk relationship update operations
/// </summary>
public class BulkRelationshipUpdateResult
{
    public bool IsSuccess { get; set; }
    public int TotalAdded { get; set; }
    public int TotalRemoved { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<Guid, RelationshipUpdateResult> OperationResults { get; set; } = new();
    
    public static BulkRelationshipUpdateResult Success(
        int totalAdded, 
        int totalRemoved, 
        int successful, 
        Dictionary<Guid, RelationshipUpdateResult> results,
        string? message = null) =>
        new() 
        { 
            IsSuccess = true, 
            TotalAdded = totalAdded, 
            TotalRemoved = totalRemoved,
            SuccessfulOperations = successful,
            FailedOperations = 0,
            OperationResults = results,
            Message = message ?? $"Bulk operation completed: {successful} successful, added {totalAdded}, removed {totalRemoved}" 
        };
    
    public static BulkRelationshipUpdateResult PartialSuccess(
        int totalAdded,
        int totalRemoved,
        int successful,
        int failed,
        Dictionary<Guid, RelationshipUpdateResult> results,
        List<string> errors) =>
        new()
        {
            IsSuccess = false,
            TotalAdded = totalAdded,
            TotalRemoved = totalRemoved,
            SuccessfulOperations = successful,
            FailedOperations = failed,
            OperationResults = results,
            Errors = errors,
            Message = $"Bulk operation partially completed: {successful} successful, {failed} failed"
        };
    
    public static BulkRelationshipUpdateResult Failure(string message, List<string>? errors = null) =>
        new() 
        { 
            IsSuccess = false, 
            Message = message, 
            Errors = errors ?? new(),
            FailedOperations = 0
        };
}
