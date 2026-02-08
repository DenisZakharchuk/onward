using System.Linq.Expressions;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.Domain.SearchProviders;

/// <summary>
/// Provides search query expressions for Category entities
/// </summary>
public class CategorySearchProvider : ISearchQueryProvider<Category, CategorySearchDTO>
{
    public Expression<Func<Category, bool>> GetSearchExpression(CategorySearchDTO searchDto)
    {
        if (searchDto == null) throw new ArgumentNullException(nameof(searchDto));
        
        return entity =>
            (string.IsNullOrEmpty(searchDto.Name) || entity.Name.Contains(searchDto.Name)) &&
            (!searchDto.ParentCategoryId.HasValue || entity.ParentCategoryId == searchDto.ParentCategoryId.Value) &&
            (!searchDto.IsActive.HasValue || entity.IsActive == searchDto.IsActive.Value);
    }
}
