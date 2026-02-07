using System.Linq.Expressions;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Provides type-safe property access for entities.
/// Abstracts property access patterns for use in mappers, validators, and relationship managers.
/// </summary>
/// <typeparam name="TEntity">Entity type containing the property</typeparam>
/// <typeparam name="TProperty">Property type</typeparam>
public interface IPropertyAccessor<TEntity, TProperty>
    where TEntity : class
{
    /// <summary>
    /// Expression to access the property. Used for LINQ query translation (e.g., EF Core).
    /// Example: entity => entity.UserId
    /// </summary>
    Expression<Func<TEntity, TProperty>> PropertyExpression { get; }

    /// <summary>
    /// Compiled getter function for efficient runtime property access.
    /// Lazily compiled from PropertyExpression on first access.
    /// </summary>
    Func<TEntity, TProperty> CompiledGetter { get; }

    /// <summary>
    /// Property name extracted from the expression.
    /// Used for error messages, logging, and debugging.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Gets the property value from an entity instance.
    /// </summary>
    /// <param name="entity">Entity instance to read from</param>
    /// <returns>Property value</returns>
    TProperty GetValue(TEntity entity);
}
