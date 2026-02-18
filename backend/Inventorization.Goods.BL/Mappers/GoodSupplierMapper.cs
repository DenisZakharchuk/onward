using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.BL.Mappers;

/// <summary>
/// Maps GoodSupplier relationships to GoodSupplierDetailsDTO
/// </summary>
public class GoodSupplierMapper : IMapper<GoodSupplier, GoodSupplierDetailsDTO>
{
    public GoodSupplierDetailsDTO Map(GoodSupplier entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        
        return new GoodSupplierDetailsDTO
        {
            Id = entity.Id,
            GoodId = entity.GoodId,
            SupplierId = entity.SupplierId,
            SupplierPrice = entity.SupplierPrice,
            LeadTimeDays = entity.LeadTimeDays,
            IsPreferred = entity.IsPreferred,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    public Expression<Func<GoodSupplier, GoodSupplierDetailsDTO>> GetProjection()
    {
        return entity => new GoodSupplierDetailsDTO
        {
            Id = entity.Id,
            GoodId = entity.GoodId,
            SupplierId = entity.SupplierId,
            SupplierPrice = entity.SupplierPrice,
            LeadTimeDays = entity.LeadTimeDays,
            IsPreferred = entity.IsPreferred,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
