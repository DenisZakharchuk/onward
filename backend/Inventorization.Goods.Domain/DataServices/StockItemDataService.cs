using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for StockItem entity
/// </summary>
public interface IStockItemDataService : IDataService<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>
{
}

/// <summary>
/// Data service implementation for StockItem entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class StockItemDataService : DataServiceBase<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>, IStockItemDataService
{
    public StockItemDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<StockItem> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
