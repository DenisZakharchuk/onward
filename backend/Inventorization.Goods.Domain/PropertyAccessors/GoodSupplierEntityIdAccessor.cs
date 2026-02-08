using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.PropertyAccessors;

/// <summary>
/// Property accessor for GoodSupplier.EntityId (GoodId)
/// Used for managing the Good side of the GoodSupplier junction entity
/// </summary>
public class GoodSupplierEntityIdAccessor : IEntityIdPropertyAccessor<GoodSupplier>
{
    private readonly PropertyAccessor<GoodSupplier, Guid> _accessor;

    public GoodSupplierEntityIdAccessor()
    {
        _accessor = new PropertyAccessor<GoodSupplier, Guid>(gs => gs.EntityId);
    }

    public Expression<Func<GoodSupplier, Guid>> PropertyExpression => _accessor.PropertyExpression;
    public Func<GoodSupplier, Guid> CompiledGetter => _accessor.CompiledGetter;
    public string PropertyName => _accessor.PropertyName;
    public Guid GetValue(GoodSupplier entity) => _accessor.GetValue(entity);
}
