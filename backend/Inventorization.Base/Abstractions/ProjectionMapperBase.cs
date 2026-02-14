using Inventorization.Base.ADTs;
using System.Linq.Expressions;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Abstract base class for projection mappers implementing common logic.
/// Uses template method pattern to allow derived classes to customize specific behavior.
/// Note: This mapper handles regular projections only. Field transformations should be
/// handled separately by using ProjectionExpressionBuilder directly.
/// </summary>
/// <typeparam name="TEntity">The source entity type</typeparam>
/// <typeparam name="TProjection">The target projection DTO type</typeparam>
public abstract class ProjectionMapperBase<TEntity, TProjection> : IProjectionMapper<TEntity, TProjection>
    where TEntity : class
    where TProjection : class, new()
{
    protected ProjectionMapperBase()
    {
    }

    /// <summary>
    /// Creates a projection expression based on requested fields.
    /// Implements the template method pattern.
    /// Note: Field transformations are not supported by this mapper - use ProjectionExpressionBuilder directly.
    /// </summary>
    public virtual Expression<Func<TEntity, TProjection>> GetProjectionExpression(ProjectionRequest? projection)
    {
        // Field transformations are not supported by regular projection mappers
        // The search service should detect transformations and use ProjectionExpressionBuilder directly
        if (projection?.FieldTransformations != null && projection.FieldTransformations.Count > 0)
        {
            throw new InvalidOperationException(
                "Field transformations cannot be handled by projection mappers. " +
                "The search service should detect transformations and use ProjectionExpressionBuilder directly to create TransformationResult.");
        }
        
        // If no specific projection requested or IsAllFields is true
        if (projection == null || projection.IsAllFields)
        {
            return GetAllFieldsProjection(
                projection?.IncludeRelatedDeep ?? false, 
                projection?.Depth ?? ProjectionRequest.DefaultDepth);
        }
        
        // If fields list is empty, return all fields (non-deep)
        if (projection.Fields.Count == 0)
        {
            return GetAllFieldsProjection(deep: false, depth: 1);
        }
        
        // Build selective projection based on requested fields
        return BuildSelectiveProjection(projection);
    }

    /// <summary>
    /// Maps an in-memory entity to projection DTO.
    /// Implements the template method pattern.
    /// </summary>
    public virtual TProjection Map(TEntity entity, ProjectionRequest? projection = null, int currentDepth = 0)
    {
        var result = new TProjection();
        
        // If no projection or IsAllFields is true, map all fields
        if (projection == null || projection.IsAllFields)
        {
            var deep = projection?.IncludeRelatedDeep ?? false;
            var maxDepth = projection?.Depth ?? ProjectionRequest.DefaultDepth;
            
            MapAllFields(entity, result, deep, maxDepth, currentDepth);
            return result;
        }
        
        // If fields list is empty, map all fields (non-deep)
        if (projection.Fields.Count == 0)
        {
            MapAllFields(entity, result, deep: false, maxDepth: 1, currentDepth: 0);
            return result;
        }
        
        // Map only requested fields
        foreach (var field in projection.Fields)
        {
            MapField(entity, result, field, currentDepth);
        }
        
        return result;
    }

    /// <summary>
    /// Returns projection expression for all fields with optional deep navigation.
    /// Must be implemented by derived classes for entity-specific field mapping.
    /// </summary>
    /// <param name="deep">Whether to include related entities</param>
    /// <param name="depth">Maximum depth for nested entities</param>
    /// <returns>Expression tree for all fields projection</returns>
    protected abstract Expression<Func<TEntity, TProjection>> GetAllFieldsProjection(bool deep, int depth);

    /// <summary>
    /// Builds projection expression for selective fields.
    /// Must be implemented by derived classes for entity-specific field selection.
    /// </summary>
    /// <param name="projection">Projection specification with selected fields</param>
    /// <returns>Expression tree for selective projection</returns>
    protected abstract Expression<Func<TEntity, TProjection>> BuildSelectiveProjection(ProjectionRequest projection);

    /// <summary>
    /// Maps all fields from entity to projection DTO for in-memory mapping.
    /// Must be implemented by derived classes for entity-specific field assignment.
    /// </summary>
    /// <param name="entity">Source entity</param>
    /// <param name="result">Target projection DTO</param>
    /// <param name="deep">Whether to include related entities</param>
    /// <param name="maxDepth">Maximum depth for nested entities</param>
    /// <param name="currentDepth">Current recursion depth level</param>
    protected abstract void MapAllFields(TEntity entity, TProjection result, bool deep, int maxDepth, int currentDepth);

    /// <summary>
    /// Maps a single field from entity to projection DTO.
    /// Must be implemented by derived classes for entity-specific field mapping.
    /// </summary>
    /// <param name="entity">Source entity</param>
    /// <param name="result">Target projection DTO</param>
    /// <param name="field">Field specification to map</param>
    /// <param name="currentDepth">Current recursion depth level</param>
    protected abstract void MapField(TEntity entity, TProjection result, FieldProjection field, int currentDepth);
}
