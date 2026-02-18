using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.BL.Modifiers;

/// <summary>
/// Updates Good entities from UpdateGoodDTO
/// </summary>
public class GoodModifier : IEntityModifier<Good, UpdateGoodDTO>
{
    public void Modify(Good entity, UpdateGoodDTO dto)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        // Use the entity's Update method to maintain immutability pattern
        entity.Update(
            name: dto.Name,
            description: dto.Description,
            sku: dto.Sku,
            unitPrice: dto.UnitPrice,
            quantityInStock: dto.QuantityInStock,
            unitOfMeasure: dto.UnitOfMeasure
        );
        
        // Handle IsActive separately
        if (dto.IsActive && !entity.IsActive)
        {
            entity.Activate();
        }
        else if (!dto.IsActive && entity.IsActive)
        {
            entity.Deactivate();
        }
    }
}
