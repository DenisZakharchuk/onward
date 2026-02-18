using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.BL.SearchProviders;

/// <summary>
/// Provides search query expressions for PurchaseOrderItem entities
/// </summary>
public class PurchaseOrderItemSearchProvider : ISearchQueryProvider<PurchaseOrderItem, PurchaseOrderItemSearchDTO>
{
    public Expression<Func<PurchaseOrderItem, bool>> GetSearchExpression(PurchaseOrderItemSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (!searchDto.PurchaseOrderId.HasValue || entity.PurchaseOrderId == searchDto.PurchaseOrderId.Value) &&
            (!searchDto.GoodId.HasValue || entity.GoodId == searchDto.GoodId.Value) &&
            (!searchDto.IsFullyReceived.HasValue || entity.IsFullyReceived == searchDto.IsFullyReceived.Value);
    }
}
