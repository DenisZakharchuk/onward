using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Manages Good ↔ Supplier many-to-many relationships via GoodSupplier junction entity.
/// Supports supplier pricing and lead time metadata.
/// </summary>
public class GoodSupplierRelationshipManager : RelationshipManagerBase<Good, Supplier, GoodSupplier>
{
    public GoodSupplierRelationshipManager(
        IRepository<Good> entityRepository,
        IRepository<Supplier> relatedEntityRepository,
        IRepository<GoodSupplier> junctionRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<GoodSupplierRelationshipManager> logger)
        : base(entityRepository, relatedEntityRepository, junctionRepository, unitOfWork,
               serviceProvider, logger, DataModelRelationships.GoodSuppliers)
    {
    }

    /// <summary>
    /// Override CreateJunctionEntity to handle GoodSupplier metadata (price and lead time).
    /// Default values are used if metadata is not provided.
    /// </summary>
    protected override Func<Guid, Guid, GoodSupplier> CreateJunctionEntity => (goodId, supplierId) =>
    {
        // Default values when metadata is not available
        // In practice, these should be provided via metadata parameter in AddRelatedAsync
        return new GoodSupplier(goodId, supplierId, supplierPrice: 0m, leadTimeDays: 0);
    };
}
