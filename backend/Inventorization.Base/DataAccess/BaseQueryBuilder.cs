using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Base.Ownership;
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
        
        var expressions = new List<Expression>(4);
        foreach (var expr in and.Expressions)
            expressions.Add(BuildExpressionBody(expr, parameter));
        
        return expressions.Aggregate((left, right) => Expression.AndAlso(left, right));
    }
    
    private Expression BuildOrExpression(OrFilter or, ParameterExpression parameter)
    {
        if (or.Expressions.Count == 0)
            return Expression.Constant(false);
        
        var expressions = new List<Expression>(4);
        foreach (var expr in or.Expressions)
            expressions.Add(BuildExpressionBody(expr, parameter));
        
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
        var values = new List<Expression>(20);
        foreach (var v in condition.Values)
            values.Add(Expression.Constant(v, property.Type));
        
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

/// <summary>
/// Ownership-aware base query builder.
/// Extends <see cref="BaseQueryBuilder{TEntity}"/> with the ability to append an
/// ownership predicate derived from the injected
/// <see cref="ICurrentIdentityContext{TOwnership}"/> — keeping ownership filtering
/// inside the query pipeline rather than relying on global EF query filters.
/// </summary>
/// <remarks>
/// Bounded contexts derive a concrete query builder per entity:
/// <code>
/// public class OrderQueryBuilder : BaseQueryBuilder&lt;Order, UserTenantOwnership&gt;
/// {
///     public OrderQueryBuilder(ICurrentIdentityContext&lt;UserTenantOwnership&gt; identityContext)
///         : base(identityContext) { }
/// }
/// </code>
/// Override <see cref="BuildOwnershipPredicate"/> to apply custom visibility rules
/// (e.g. allow all users in the same tenant to read each other's records).
/// </remarks>
/// <typeparam name="TEntity">The entity type to build queries for.</typeparam>
/// <typeparam name="TOwnership">Concrete ownership VO for this bounded context.</typeparam>
public abstract class BaseQueryBuilder<TEntity, TOwnership>
    : BaseQueryBuilder<TEntity>, IQueryBuilder<TEntity, TOwnership>
    where TEntity : class
    where TOwnership : OwnershipValueObject
{
    private readonly ICurrentIdentityContext<TOwnership> _identityContext;

    protected BaseQueryBuilder(ICurrentIdentityContext<TOwnership> identityContext)
    {
        _identityContext = identityContext ?? throw new ArgumentNullException(nameof(identityContext));
    }

    /// <inheritdoc />
    public IQueryable<TEntity> BuildOwnedQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery)
    {
        // If entity does not participate in ownership, fall back to standard query
        if (!typeof(IOwnedEntity<TOwnership>).IsAssignableFrom(typeof(TEntity)))
            return BuildQuery(baseQuery, searchQuery);

        // Anonymous callers get no results for owned entities
        if (!_identityContext.IsAuthenticated || _identityContext.Ownership is null)
            return baseQuery.Where(_ => false);

        // Build standard query first, then append ownership predicate
        var query = BuildQuery(baseQuery, searchQuery);
        var ownershipPredicate = BuildOwnershipPredicate(_identityContext.Ownership);
        return query.Where(ownershipPredicate);
    }

    /// <summary>
    /// Builds the LINQ predicate that restricts results to entities the current
    /// caller is permitted to see based on their ownership VO.
    /// </summary>
    /// <remarks>
    /// The default implementation performs an exact-match check between the entity's
    /// <see cref="IOwnedEntity{TOwnership}.Ownership"/> and the caller's
    /// <paramref name="ownership"/> VO (record equality).
    /// Override to implement broader access rules — for example, allowing all members
    /// of the same tenant to read records owned by any user in that tenant.
    /// </remarks>
    protected virtual Expression<Func<TEntity, bool>> BuildOwnershipPredicate(TOwnership ownership)
    {
        var parameter = Expression.Parameter(typeof(TEntity), ParameterName);

        // entity.Ownership == ownership  (record structural equality via EqualityContract)
        var ownershipProperty = Expression.Property(parameter, nameof(IOwnedEntity<TOwnership>.Ownership));
        var ownershipConstant = Expression.Constant(ownership, typeof(TOwnership));
        var equalsCall = Expression.Equal(ownershipProperty, ownershipConstant);

        return Expression.Lambda<Func<TEntity, bool>>(equalsCall, parameter);
    }
}
