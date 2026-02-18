using Inventorization.Base.Abstractions;
using Inventorization.Base.ADTs;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.ADTs;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.Services;

/// <summary>
/// Service for executing ADT-based search queries on Category entities.
/// Inherits all search logic from BaseSearchService.
/// Supports both regular projections and field transformations.
/// </summary>
public class CategorySearchService : BaseSearchService<Category, CategoryProjection>
{
    public CategorySearchService(
        IRepository<Category> repository,
        IQueryBuilder<Category> queryBuilder,
        IProjectionMapper<Category, CategoryProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger<CategorySearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, logger)
    {
    }
}
