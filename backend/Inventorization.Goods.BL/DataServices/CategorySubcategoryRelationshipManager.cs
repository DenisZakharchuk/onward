using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.BL.PropertyAccessors;
using Microsoft.Extensions.Logging;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Manages Category → Category self-referencing relationships (parent/child).
/// A category can contain multiple subcategories.
/// </summary>
public class CategorySubcategoryRelationshipManager : OneToManyRelationshipManagerBase<Category, Category>
{
    public CategorySubcategoryRelationshipManager(
        IRepository<Category> parentRepository,
        IRepository<Category> childRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<CategorySubcategoryRelationshipManager> logger)
        : base(parentRepository, childRepository, unitOfWork, serviceProvider, logger,
               DataModelRelationships.CategorySubCategories, typeof(CategoryParentIdAccessor))
    {
    }

    protected override void SetParentId(Category child, Guid? parentId)
    {
        var property = typeof(Category).GetProperty("ParentCategoryId");
        property?.SetValue(child, parentId);
    }
}
