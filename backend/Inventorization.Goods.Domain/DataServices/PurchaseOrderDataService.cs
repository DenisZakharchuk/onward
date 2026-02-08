using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.PurchaseOrder;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for PurchaseOrder entity
/// </summary>
public interface IPurchaseOrderDataService : IDataService<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>
{
}

/// <summary>
/// Data service implementation for PurchaseOrder entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class PurchaseOrderDataService : DataServiceBase<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>, IPurchaseOrderDataService
{
    public PurchaseOrderDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<PurchaseOrder> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<PurchaseOrder, CreatePurchaseOrderDTO, UpdatePurchaseOrderDTO, DeletePurchaseOrderDTO, PurchaseOrderDetailsDTO, PurchaseOrderSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
