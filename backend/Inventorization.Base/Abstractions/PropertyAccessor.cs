using System.Linq.Expressions;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Default implementation of IPropertyAccessor providing cached property access.
/// </summary>
/// <typeparam name="TEntity">Entity type containing the property</typeparam>
/// <typeparam name="TProperty">Property type</typeparam>
public class PropertyAccessor<TEntity, TProperty> : IPropertyAccessor<TEntity, TProperty>
    where TEntity : class
{
    private Func<TEntity, TProperty>? _compiledGetter;
    private string? _propertyName;

    public PropertyAccessor(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        PropertyExpression = propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression));
    }

    public Expression<Func<TEntity, TProperty>> PropertyExpression { get; }

    public Func<TEntity, TProperty> CompiledGetter
    {
        get
        {
            // Lazy compilation with thread-safe initialization
            if (_compiledGetter == null)
            {
                _compiledGetter = PropertyExpression.Compile();
            }
            return _compiledGetter;
        }
    }

    public string PropertyName
    {
        get
        {
            if (_propertyName == null)
            {
                // Extract property name from expression
                if (PropertyExpression.Body is MemberExpression memberExpression)
                {
                    _propertyName = memberExpression.Member.Name;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expression must be a member access expression. Got: {PropertyExpression.Body.GetType().Name}");
                }
            }
            return _propertyName;
        }
    }

    public TProperty GetValue(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return CompiledGetter(entity);
    }
}
