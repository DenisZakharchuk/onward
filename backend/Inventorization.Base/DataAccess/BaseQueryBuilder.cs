using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// Base implementation for converting SearchQuery ADT to IQueryable for any entity.
/// Builds LINQ expressions from filter conditions, projections, and sorting.
/// Derived classes specify the entity type and parameter name.
/// </summary>
/// <typeparam name="TEntity">The entity type to build queries for</typeparam>
public abstract class BaseQueryBuilder<TEntity> : IQueryBuilder<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the parameter name to use in LINQ expressions (e.g., "g" for Good, "c" for Category)
    /// Default implementation uses the first character of entity name in lowercase
    /// </summary>
    protected virtual string ParameterName => typeof(TEntity).Name.ToLower()[0].ToString();
    
    /// <summary>
    /// Converts FilterExpression ADT to LINQ Where expression
    /// </summary>
    public Expression<Func<TEntity, bool>>? BuildFilterExpression(FilterExpression? filter)
    {
        if (filter == null)
            return null;
        
        return BuildExpression(filter);
    }
    
    /// <summary>
    /// Applies projection to queryable (Select clause with includes)
    /// </summary>
    public IQueryable<TEntity> ApplyProjection(IQueryable<TEntity> query, ProjectionRequest? projection)
    {
        if (projection == null || projection.Fields.Count == 0)
            return query;
        
        // Apply includes for related entities
        var relatedPaths = projection.Fields
            .Where(f => f.IsRelated && !string.IsNullOrEmpty(f.RelationPath))
            .Select(f => f.RelationPath!)
            .Distinct();
        
        foreach (var path in relatedPaths)
        {
            query = query.Include(path);
        }
        
        return query;
    }
    
    /// <summary>
    /// Applies sorting to queryable (OrderBy/ThenBy clauses)
    /// </summary>
    public IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortRequest? sort)
    {
        if (sort == null || sort.Fields.Count == 0)
            return query;
        
        IOrderedQueryable<TEntity>? orderedQuery = null;
        
        for (int i = 0; i < sort.Fields.Count; i++)
        {
            var sortField = sort.Fields[i];
            var parameter = Expression.Parameter(typeof(TEntity), ParameterName);
            var property = Expression.Property(parameter, sortField.FieldName);
            var lambda = Expression.Lambda(property, parameter);
            
            if (i == 0)
            {
                // First sort field uses OrderBy/OrderByDescending
                orderedQuery = sortField.Direction == SortDirection.Ascending
                    ? Queryable.OrderBy(query, (dynamic)lambda)
                    : Queryable.OrderByDescending(query, (dynamic)lambda);
            }
            else
            {
                // Subsequent sort fields use ThenBy/ThenByDescending
                orderedQuery = sortField.Direction == SortDirection.Ascending
                    ? Queryable.ThenBy(orderedQuery!, (dynamic)lambda)
                    : Queryable.ThenByDescending(orderedQuery!, (dynamic)lambda);
            }
        }
        
        return orderedQuery ?? query;
    }
    
    /// <summary>
    /// Builds complete queryable from SearchQuery ADT
    /// </summary>
    public IQueryable<TEntity> BuildQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery)
    {
        var query = baseQuery;
        
        // Apply filter
        var filterExpression = BuildFilterExpression(searchQuery.Filter);
        if (filterExpression != null)
        {
            query = query.Where(filterExpression);
        }
        
        // Apply projection (includes)
        query = ApplyProjection(query, searchQuery.Projection);
        
        // Apply sorting
        query = ApplySorting(query, searchQuery.Sort);
        
        return query;
    }
    
    #region Private Expression Builders
    
    private Expression<Func<TEntity, bool>> BuildExpression(FilterExpression filter)
    {
        var parameter = Expression.Parameter(typeof(TEntity), ParameterName);
        var body = BuildExpressionBody(filter, parameter);
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
    
    private Expression BuildExpressionBody(FilterExpression filter, ParameterExpression parameter)
    {
        return filter switch
        {
            LeafFilter leaf => BuildConditionExpression(leaf.Condition, parameter),
            AndFilter and => BuildAndExpression(and, parameter),
            OrFilter or => BuildOrExpression(or, parameter),
            _ => throw new NotSupportedException($"Filter expression type {filter.GetType().Name} is not supported")
        };
    }
    
    private Expression BuildAndExpression(AndFilter and, ParameterExpression parameter)
    {
        if (and.Expressions.Count == 0)
            return Expression.Constant(true);
        
        var expressions = and.Expressions.Select(e => BuildExpressionBody(e, parameter)).ToList();
        
        return expressions.Aggregate((left, right) => Expression.AndAlso(left, right));
    }
    
    private Expression BuildOrExpression(OrFilter or, ParameterExpression parameter)
    {
        if (or.Expressions.Count == 0)
            return Expression.Constant(false);
        
        var expressions = or.Expressions.Select(e => BuildExpressionBody(e, parameter)).ToList();
        
        return expressions.Aggregate((left, right) => Expression.OrElse(left, right));
    }
    
    private Expression BuildConditionExpression(FilterCondition condition, ParameterExpression parameter)
    {
        return condition switch
        {
            EqualsCondition eq => BuildEqualsExpression(eq, parameter),
            GreaterThanCondition gt => BuildGreaterThanExpression(gt, parameter),
            LessThanCondition lt => BuildLessThanExpression(lt, parameter),
            GreaterThanOrEqualCondition gte => BuildGreaterThanOrEqualExpression(gte, parameter),
            LessThanOrEqualCondition lte => BuildLessThanOrEqualExpression(lte, parameter),
            ContainsCondition contains => BuildContainsExpression(contains, parameter),
            StartsWithCondition startsWith => BuildStartsWithExpression(startsWith, parameter),
            InCondition inCondition => BuildInExpression(inCondition, parameter),
            IsNullCondition isNull => BuildIsNullExpression(isNull, parameter),
            IsNotNullCondition isNotNull => BuildIsNotNullExpression(isNotNull, parameter),
            _ => throw new NotSupportedException($"Filter condition type {condition.GetType().Name} is not supported")
        };
    }
    
    private Expression BuildEqualsExpression(EqualsCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(condition.Value, property.Type);
        return Expression.Equal(property, value);
    }
    
    private Expression BuildGreaterThanExpression(GreaterThanCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(Convert.ChangeType(condition.Value, property.Type), property.Type);
        return Expression.GreaterThan(property, value);
    }
    
    private Expression BuildLessThanExpression(LessThanCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(Convert.ChangeType(condition.Value, property.Type), property.Type);
        return Expression.LessThan(property, value);
    }
    
    private Expression BuildGreaterThanOrEqualExpression(GreaterThanOrEqualCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(Convert.ChangeType(condition.Value, property.Type), property.Type);
        return Expression.GreaterThanOrEqual(property, value);
    }
    
    private Expression BuildLessThanOrEqualExpression(LessThanOrEqualCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(Convert.ChangeType(condition.Value, property.Type), property.Type);
        return Expression.LessThanOrEqual(property, value);
    }
    
    private Expression BuildContainsExpression(ContainsCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(condition.Value);
        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        return Expression.Call(property, method, value);
    }
    
    private Expression BuildStartsWithExpression(StartsWithCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var value = Expression.Constant(condition.Value);
        var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
        return Expression.Call(property, method, value);
    }
    
    private Expression BuildInExpression(InCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var values = condition.Values.Select(v => Expression.Constant(v, property.Type)).ToList();
        
        if (values.Count == 0)
            return Expression.Constant(false);
        
        // Build: value == item1 || value == item2 || ...
        return values
            .Select(v => Expression.Equal(property, v))
            .Aggregate((left, right) => Expression.OrElse(left, right));
    }
    
    private Expression BuildIsNullExpression(IsNullCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var nullValue = Expression.Constant(null, property.Type);
        return Expression.Equal(property, nullValue);
    }
    
    private Expression BuildIsNotNullExpression(IsNotNullCondition condition, ParameterExpression parameter)
    {
        var property = Expression.Property(parameter, condition.FieldName);
        var nullValue = Expression.Constant(null, property.Type);
        return Expression.NotEqual(property, nullValue);
    }
    
    #endregion
}
