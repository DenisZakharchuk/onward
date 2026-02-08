using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.GoodSupplier;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for GoodSupplier relationship
/// </summary>
public interface IGoodSupplierDataService : IDataService<GoodSupplier, CreateGoodSupplierDTO, UpdateGoodSupplierDTO, DeleteGoodSupplierDTO, GoodSupplierDetailsDTO, GoodSupplierSearchDTO>
{
}

/// <summary>
/// Data service implementation for GoodSupplier relationship
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class GoodSupplierDataService : DataServiceBase<GoodSupplier, CreateGoodSupplierDTO, UpdateGoodSupplierDTO, DeleteGoodSupplierDTO, GoodSupplierDetailsDTO, GoodSupplierSearchDTO>, IGoodSupplierDataService
{
    public GoodSupplierDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<GoodSupplier> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<GoodSupplier, CreateGoodSupplierDTO, UpdateGoodSupplierDTO, DeleteGoodSupplierDTO, GoodSupplierDetailsDTO, GoodSupplierSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
