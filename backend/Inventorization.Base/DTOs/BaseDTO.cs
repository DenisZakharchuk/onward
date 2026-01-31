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
public abstract class UpdateDTO
{
    public Guid Id { get; set; }
}

/// <summary>
/// Base class for Delete DTOs
/// </summary>
public abstract class DeleteDTO
{
    public Guid Id { get; set; }
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
