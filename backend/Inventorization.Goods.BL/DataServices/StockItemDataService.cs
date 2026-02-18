using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.StockItem;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Data service interface for StockItem entity
/// </summary>
public interface IStockItemDataService : IDataService<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, InitStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>
{
}

/// <summary>
/// Data service implementation for StockItem entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class StockItemDataService : DataServiceBase<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, InitStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>, IStockItemDataService
{
    public StockItemDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<StockItem> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<StockItem, CreateStockItemDTO, UpdateStockItemDTO, DeleteStockItemDTO, InitStockItemDTO, StockItemDetailsDTO, StockItemSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
