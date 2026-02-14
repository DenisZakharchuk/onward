using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.ADTs;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.Domain.Services;

/// <summary>
/// Service for executing ADT-based search queries on Good entities.
/// Inherits all search logic from BaseSearchService.
/// Supports both regular projections and field transformations.
/// </summary>
public class GoodSearchService : BaseSearchService<Good, GoodProjection>
{
    public GoodSearchService(
        IRepository<Good> repository,
        IQueryBuilder<Good> queryBuilder,
        IProjectionMapper<Good, GoodProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger<GoodSearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, logger)
    {
    }
}
