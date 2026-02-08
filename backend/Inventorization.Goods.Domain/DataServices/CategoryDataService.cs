using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Category;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for Category entity
/// </summary>
public interface ICategoryDataService : IDataService<Category, CreateCategoryDTO, UpdateCategoryDTO, DeleteCategoryDTO, CategoryDetailsDTO, CategorySearchDTO>
{
}

/// <summary>
/// Data service implementation for Category entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class CategoryDataService : DataServiceBase<Category, CreateCategoryDTO, UpdateCategoryDTO, DeleteCategoryDTO, CategoryDetailsDTO, CategorySearchDTO>, ICategoryDataService
{
    public CategoryDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Category> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Category, CreateCategoryDTO, UpdateCategoryDTO, DeleteCategoryDTO, CategoryDetailsDTO, CategorySearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
