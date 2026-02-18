using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Data service interface for StockLocation entity
/// </summary>
public interface IStockLocationDataService : IDataService<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, InitStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>
{
}

/// <summary>
/// Data service implementation for StockLocation entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class StockLocationDataService : DataServiceBase<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, InitStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>, IStockLocationDataService
{
    public StockLocationDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<StockLocation> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, InitStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
