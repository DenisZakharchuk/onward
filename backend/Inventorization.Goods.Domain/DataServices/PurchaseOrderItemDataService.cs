using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrderItem;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for PurchaseOrderItem entity
/// </summary>
public interface IPurchaseOrderItemDataService : IDataService<PurchaseOrderItem, CreatePurchaseOrderItemDTO, UpdatePurchaseOrderItemDTO, DeletePurchaseOrderItemDTO, InitPurchaseOrderItemDTO, PurchaseOrderItemDetailsDTO, PurchaseOrderItemSearchDTO>
{
}

/// <summary>
/// Data service implementation for PurchaseOrderItem entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class PurchaseOrderItemDataService : DataServiceBase<PurchaseOrderItem, CreatePurchaseOrderItemDTO, UpdatePurchaseOrderItemDTO, DeletePurchaseOrderItemDTO, InitPurchaseOrderItemDTO, PurchaseOrderItemDetailsDTO, PurchaseOrderItemSearchDTO>, IPurchaseOrderItemDataService
{
    public PurchaseOrderItemDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<PurchaseOrderItem> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<PurchaseOrderItem, CreatePurchaseOrderItemDTO, UpdatePurchaseOrderItemDTO, DeletePurchaseOrderItemDTO, InitPurchaseOrderItemDTO, PurchaseOrderItemDetailsDTO, PurchaseOrderItemSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
