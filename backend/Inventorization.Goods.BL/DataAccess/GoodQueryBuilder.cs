using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.DataAccess;

/// <summary>
/// Converts SearchQuery ADT to IQueryable for Good entity.
/// Inherits all query building logic from BaseQueryBuilder.
/// Uses default parameter name "g" (first letter of Good).
/// </summary>
public class GoodQueryBuilder : BaseQueryBuilder<Good>
{
}
