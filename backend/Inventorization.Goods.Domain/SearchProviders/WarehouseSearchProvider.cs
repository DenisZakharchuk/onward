using System.Linq.Expressions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for Warehouse entities
/// </summary>
public class WarehouseSearchProvider : ISearchQueryProvider<Warehouse, WarehouseSearchDTO>
{
    public Expression<Func<Warehouse, bool>> GetSearchExpression(WarehouseSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (string.IsNullOrEmpty(searchDto.Name) || entity.Name.Contains(searchDto.Name)) &&
            (string.IsNullOrEmpty(searchDto.Code) || entity.Code.Contains(searchDto.Code)) &&
            (string.IsNullOrEmpty(searchDto.City) || (entity.City != null && entity.City.Contains(searchDto.City))) &&
            (string.IsNullOrEmpty(searchDto.Country) || (entity.Country != null && entity.Country.Contains(searchDto.Country))) &&
            (!searchDto.IsActive.HasValue || entity.IsActive == searchDto.IsActive.Value);
    }
}
