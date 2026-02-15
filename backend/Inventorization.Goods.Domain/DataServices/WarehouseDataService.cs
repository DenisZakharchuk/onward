using Inventorization.Goods.Domain.Entities;
using Inventorization.Goods.DTO.DTO.Warehouse;

namespace Inventorization.Goods.Domain.DataServices;

/// <summary>
/// Data service interface for Warehouse entity
/// </summary>
public interface IWarehouseDataService : IDataService<Warehouse, CreateWarehouseDTO, UpdateWarehouseDTO, DeleteWarehouseDTO, InitWarehouseDTO, WarehouseDetailsDTO, WarehouseSearchDTO>
{
}

/// <summary>
/// Data service implementation for Warehouse entity
/// Inherits all CRUD operations from DataServiceBase
/// </summary>
public class WarehouseDataService : DataServiceBase<Warehouse, CreateWarehouseDTO, UpdateWarehouseDTO, DeleteWarehouseDTO, InitWarehouseDTO, WarehouseDetailsDTO, WarehouseSearchDTO>, IWarehouseDataService
{
    public WarehouseDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Warehouse> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Warehouse, CreateWarehouseDTO, UpdateWarehouseDTO, DeleteWarehouseDTO, InitWarehouseDTO, WarehouseDetailsDTO, WarehouseSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
