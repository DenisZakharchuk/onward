using Inventorization.Base.DataAccess;
using Inventorization.Goods.Domain.Entities;

namespace Inventorization.Goods.Domain.DataAccess;

/// <summary>
/// Converts SearchQuery ADT to IQueryable for Good entity.
/// Inherits all query building logic from BaseQueryBuilder.
/// Uses default parameter name "g" (first letter of Good).
/// </summary>
public class GoodQueryBuilder : BaseQueryBuilder<Good>
{
}
