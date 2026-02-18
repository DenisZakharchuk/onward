using System.Linq.Expressions;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.BL.SearchProviders;

/// <summary>
/// Provides search query expressions for Supplier entities
/// </summary>
public class SupplierSearchProvider : ISearchQueryProvider<Supplier, SupplierSearchDTO>
{
    public Expression<Func<Supplier, bool>> GetSearchExpression(SupplierSearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (string.IsNullOrEmpty(searchDto.Name) || entity.Name.Contains(searchDto.Name)) &&
            (string.IsNullOrEmpty(searchDto.ContactEmail) || entity.ContactEmail.Contains(searchDto.ContactEmail)) &&
            (string.IsNullOrEmpty(searchDto.City) || (entity.City != null && entity.City.Contains(searchDto.City))) &&
            (string.IsNullOrEmpty(searchDto.Country) || (entity.Country != null && entity.Country.Contains(searchDto.Country))) &&
            (!searchDto.IsActive.HasValue || entity.IsActive == searchDto.IsActive.Value);
    }
}
