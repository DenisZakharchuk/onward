using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.BL.Modifiers;

/// <summary>
/// Updates GoodSupplier relationships from UpdateGoodSupplierDTO
/// </summary>
public class GoodSupplierModifier : IEntityModifier<GoodSupplier, UpdateGoodSupplierDTO>
{
    public void Modify(GoodSupplier entity, UpdateGoodSupplierDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Update pricing information
        entity.UpdatePricing(
            supplierPrice: dto.SupplierPrice,
            leadTimeDays: dto.LeadTimeDays
        );
        
        // Update preferred status
        if (dto.IsPreferred && !entity.IsPreferred)
        {
            entity.SetAsPreferred();
        }
        else if (!dto.IsPreferred && entity.IsPreferred)
        {
            entity.RemovePreferredStatus();
        }
    }
}
