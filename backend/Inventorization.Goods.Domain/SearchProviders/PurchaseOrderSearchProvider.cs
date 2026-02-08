using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for PurchaseOrder entities
/// </summary>
public class PurchaseOrderSearchProvider : ISearchQueryProvider<PurchaseOrder, PurchaseOrderSearchDTO>
{
    public Expression<Func<PurchaseOrder, bool>> GetSearchExpression(PurchaseOrderSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (string.IsNullOrEmpty(searchDto.OrderNumber) || entity.OrderNumber.Contains(searchDto.OrderNumber)) &&
            (!searchDto.SupplierId.HasValue || entity.SupplierId == searchDto.SupplierId.Value) &&
            (!searchDto.Status.HasValue || entity.Status == searchDto.Status.Value) &&
            (!searchDto.OrderDateFrom.HasValue || entity.OrderDate >= searchDto.OrderDateFrom.Value) &&
            (!searchDto.OrderDateTo.HasValue || entity.OrderDate <= searchDto.OrderDateTo.Value) &&
            (!searchDto.ExpectedDeliveryDateFrom.HasValue || (entity.ExpectedDeliveryDate.HasValue && entity.ExpectedDeliveryDate.Value >= searchDto.ExpectedDeliveryDateFrom.Value)) &&
            (!searchDto.ExpectedDeliveryDateTo.HasValue || (entity.ExpectedDeliveryDate.HasValue && entity.ExpectedDeliveryDate.Value <= searchDto.ExpectedDeliveryDateTo.Value));
    }
}
