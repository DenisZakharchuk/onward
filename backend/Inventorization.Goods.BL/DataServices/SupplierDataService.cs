using Inventorization.Goods.BL.Entities;
using Inventorization.Goods.DTO.DTO.Supplier;

namespace Inventorization.Goods.BL.DataServices;

/// <summary>
/// Data service interface for Supplier entity
/// </summary>
public interface ISupplierDataService : IDataService<Supplier, CreateSupplierDTO, UpdateSupplierDTO, DeleteSupplierDTO, InitSupplierDTO, SupplierDetailsDTO, SupplierSearchDTO>
{
}

/// <summary>
/// Data service implementation for Supplier entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class SupplierDataService : DataServiceBase<Supplier, CreateSupplierDTO, UpdateSupplierDTO, DeleteSupplierDTO, InitSupplierDTO, SupplierDetailsDTO, SupplierSearchDTO>, ISupplierDataService
{
    public SupplierDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Supplier> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Supplier, CreateSupplierDTO, UpdateSupplierDTO, DeleteSupplierDTO, InitSupplierDTO, SupplierDetailsDTO, SupplierSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
