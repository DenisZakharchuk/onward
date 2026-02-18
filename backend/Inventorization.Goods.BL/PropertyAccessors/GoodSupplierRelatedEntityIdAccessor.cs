using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.PropertyAccessors;

/// <summary>
/// Property accessor for GoodSupplier.RelatedEntityId (SupplierId)
/// Used for managing the Supplier side of the GoodSupplier junction entity
/// </summary>
public class GoodSupplierRelatedEntityIdAccessor : IRelatedEntityIdPropertyAccessor<GoodSupplier>
{
    private readonly PropertyAccessor<GoodSupplier, Guid> _accessor;

    public GoodSupplierRelatedEntityIdAccessor()
    {
        _accessor = new PropertyAccessor<GoodSupplier, Guid>(gs => gs.RelatedEntityId);
    }

    public Expression<Func<GoodSupplier, Guid>> PropertyExpression => _accessor.PropertyExpression;
    public Func<GoodSupplier, Guid> CompiledGetter => _accessor.CompiledGetter;
    public string PropertyName => _accessor.PropertyName;
    public Guid GetValue(GoodSupplier entity) => _accessor.GetValue(entity);
}
