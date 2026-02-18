using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Manages Category → Good one-to-many relationships.
/// A category can contain multiple goods.
/// </summary>
public class CategoryGoodRelationshipManager : OneToManyRelationshipManagerBase<Category, Good>
{
    public CategoryGoodRelationshipManager(
        IRepository<Category> parentRepository,
        IRepository<Good> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<CategoryGoodRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.CategoryGoods, typeof(GoodCategoryIdAccessor))
    {
    }

    protected override void SetParentId(Good child, Guid? parentId)
    {
        var property = typeof(Good).GetProperty("CategoryId");
        property?.SetValue(child, parentId ?? Guid.Empty);
    }
}
