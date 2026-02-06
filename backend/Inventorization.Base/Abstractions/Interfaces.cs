using System.Linq.Expressions;
using Inventorization.Base.DTOs;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Generic mapper interface for entity-to-DTO mapping and LINQ projection
/// </summary>
public interface IMapper<TEntity, TDetailsDTO>
    where TEntity : class
    where TDetailsDTO : class
{
    /// <summary>
    /// Maps entity object to DTO
    /// </summary>
    TDetailsDTO Map(TEntity entity);

    /// <summary>
    /// Returns LINQ expression for projection (used in queries)
    /// </summary>
    Expression<Func<TEntity, TDetailsDTO>> GetProjection();
}

/// <summary>
/// Creates entity from CreateDTO
/// </summary>
public interface IEntityCreator<TEntity, in TCreateDTO>
    where TEntity : class
    where TCreateDTO : class
{
    /// <summary>
    /// Creates entity from DTO
    /// </summary>
    TEntity Create(TCreateDTO dto);
}

/// <summary>
/// Updates entity from UpdateDTO
/// </summary>
public interface IEntityModifier<TEntity, in TUpdateDTO>
    where TEntity : class
    where TUpdateDTO : class
{
    /// <summary>
    /// Updates entity with data from DTO
    /// </summary>
    void Modify(TEntity entity, TUpdateDTO dto);
}

/// <summary>
/// Creates LINQ expression for search queries
/// </summary>
public interface ISearchQueryProvider<TEntity, in TSearchDTO>
    where TEntity : class
    where TSearchDTO : class
{
    /// <summary>
    /// Returns LINQ expression for filtering based on search DTO
    /// </summary>
    Expression<Func<TEntity, bool>> GetSearchExpression(TSearchDTO searchDto);
}

/// <summary>
/// Generic data service interface
/// </summary>
public interface IDataService<TEntity, in TCreateDTO, in TUpdateDTO, in TDeleteDTO, TDetailsDTO, in TSearchDTO>
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : class
    where TDeleteDTO : class
    where TDetailsDTO : class
    where TSearchDTO : class
{
    Task<ServiceResult<TDetailsDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<TDetailsDTO>> AddAsync(TCreateDTO createDto, CancellationToken cancellationToken = default);
    Task<ServiceResult<TDetailsDTO>> UpdateAsync(TUpdateDTO updateDto, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(TDeleteDTO deleteDto, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<TDetailsDTO>>> SearchAsync(TSearchDTO searchDto, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic validator interface
/// </summary>
public interface IValidator<in T>
    where T : class
{
    Task<ValidationResult> ValidateAsync(T obj, CancellationToken cancellationToken = default);
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Ok() => new() { IsValid = true };
    public static ValidationResult WithErrors(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Password hashing abstraction
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Unit of Work interface for atomic commits
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Manages relationships between entities with add/remove semantics
/// </summary>
/// <typeparam name="TEntity">Parent entity type</typeparam>
/// <typeparam name="TRelatedEntity">Related entity type</typeparam>
public interface IRelationshipManager<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
{
    /// <summary>
    /// Updates relationships by adding and removing related entities for a single parent entity
    /// </summary>
    /// <param name="entityId">ID of the parent entity</param>
    /// <param name="changes">Changes to apply (IDs to add and remove)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with counts of added/removed relationships</returns>
    Task<RelationshipUpdateResult> UpdateRelationshipsAsync(
        Guid entityId, 
        EntityReferencesDTO changes, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates relationships for multiple parent entities in a single transaction
    /// </summary>
    /// <param name="changes">Dictionary mapping entity IDs to their relationship changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with aggregated counts and per-entity results</returns>
    Task<BulkRelationshipUpdateResult> UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all related entity IDs for the parent entity
    /// </summary>
    /// <param name="entityId">ID of the parent entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of related entity IDs</returns>
    Task<List<Guid>> GetRelatedIdsAsync(Guid entityId, CancellationToken cancellationToken = default);
}
