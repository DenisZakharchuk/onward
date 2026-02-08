using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for GoodSupplier relationships
/// </summary>
public class GoodSupplierSearchProvider : ISearchQueryProvider<GoodSupplier, GoodSupplierSearchDTO>
{
    public Expression<Func<GoodSupplier, bool>> GetSearchExpression(GoodSupplierSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (!searchDto.GoodId.HasValue || entity.GoodId == searchDto.GoodId.Value) &&
            (!searchDto.SupplierId.HasValue || entity.SupplierId == searchDto.SupplierId.Value) &&
            (!searchDto.IsPreferred.HasValue || entity.IsPreferred == searchDto.IsPreferred.Value) &&
            (!searchDto.MinSupplierPrice.HasValue || entity.SupplierPrice >= searchDto.MinSupplierPrice.Value) &&
            (!searchDto.MaxSupplierPrice.HasValue || entity.SupplierPrice <= searchDto.MaxSupplierPrice.Value);
    }
}
