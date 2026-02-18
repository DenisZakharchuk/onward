using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Data service interface for PurchaseOrder entity
/// </summary>
public interface IPurchaseOrderDataService : IDataService<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, InitPurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>
{
}

/// <summary>
/// Data service implementation for PurchaseOrder entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class PurchaseOrderDataService : DataServiceBase<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, InitPurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>, IPurchaseOrderDataService
{
    public PurchaseOrderDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<PurchaseOrder> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, InitPurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
