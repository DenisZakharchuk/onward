using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Good;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Data service interface for Good entity
/// </summary>
public interface IGoodDataService : IDataService<Good, CreateGoodDTO, UpdateGoodDTO, DeleteGoodDTO, InitGoodDTO, GoodDetailsDTO, GoodSearchDTO>
{
}

/// <summary>
/// Data service implementation for Good entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class GoodDataService : DataServiceBase<Good, CreateGoodDTO, UpdateGoodDTO, DeleteGoodDTO, InitGoodDTO, GoodDetailsDTO, GoodSearchDTO>, IGoodDataService
{
    public GoodDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Good> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Good, CreateGoodDTO, UpdateGoodDTO, DeleteGoodDTO, InitGoodDTO, GoodDetailsDTO, GoodSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
