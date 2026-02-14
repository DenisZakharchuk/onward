using Inventorization.Base.ADTs;
using System.Linq.Expressions;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Generic interface for mapping entities to projection DTOs with dynamic field selection.
/// Supports both EF Core queryable projection and in-memory mapping with depth control.
/// </summary>
/// <typeparam name="TEntity">The source entity type</typeparam>
/// <typeparam name="TProjection">The target projection DTO type</typeparam>
public interface IProjectionMapper<TEntity, TProjection>
    where TEntity : class
    where TProjection : class, new()
{
    /// <summary>
    /// Creates a projection expression for EF Core queryable translation.
    /// If no projection is specified, returns all fields.
    /// </summary>
    /// <param name="projection">Projection specification defining which fields to include</param>
    /// <returns>Expression tree for EF Core translation</returns>
    Expression<Func<TEntity, TProjection>> GetProjectionExpression(ProjectionRequest? projection);

    /// <summary>
    /// Maps an in-memory entity to projection DTO.
    /// Used when EF Core query translation is not possible.
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <param name="projection">Projection specification defining which fields to include</param>
    /// <param name="currentDepth">Current recursion depth level (starts at 0)</param>
    /// <returns>Projected DTO instance</returns>
    TProjection Map(TEntity entity, ProjectionRequest? projection = null, int currentDepth = 0);
}
