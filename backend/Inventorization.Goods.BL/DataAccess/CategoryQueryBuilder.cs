using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.DataAccess;

/// <summary>
/// Converts SearchQuery ADT to IQueryable for Category entity.
/// Inherits all query building logic from BaseQueryBuilder.
/// Uses default parameter name "c" (first letter of Category).
/// </summary>
public class CategoryQueryBuilder : BaseQueryBuilder<Category>
{
}
