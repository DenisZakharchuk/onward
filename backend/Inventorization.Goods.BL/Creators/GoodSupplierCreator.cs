using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.BL.Creators;

/// <summary>
/// Creates GoodSupplier relationships from CreateGoodSupplierDTO
/// </summary>
public class GoodSupplierCreator : IEntityCreator<GoodSupplier, CreateGoodSupplierDTO>
{
    public GoodSupplier Create(CreateGoodSupplierDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        
        var goodSupplier = new GoodSupplier(
            goodId: dto.GoodId,
            supplierId: dto.SupplierId,
            supplierPrice: dto.SupplierPrice,
            leadTimeDays: dto.LeadTimeDays
        );
        
        // Set preferred status if specified
        if (dto.IsPreferred)
        {
            goodSupplier.SetAsPreferred();
        }
        
        return goodSupplier;
    }
}
