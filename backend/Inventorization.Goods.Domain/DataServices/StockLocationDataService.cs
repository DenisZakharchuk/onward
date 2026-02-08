using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockLocation;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for StockLocation entity
/// </summary>
public interface IStockLocationDataService : IDataService<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>
{
}

/// <summary>
/// Data service implementation for StockLocation entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class StockLocationDataService : DataServiceBase<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>, IStockLocationDataService
{
    public StockLocationDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<StockLocation> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<StockLocation, CreateStockLocationDTO, UpdateStockLocationDTO, DeleteStockLocationDTO, StockLocationDetailsDTO, StockLocationSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
