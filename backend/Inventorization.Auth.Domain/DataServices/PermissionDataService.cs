using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.Domain.DataServices;

/// <summary>
/// Data service interface for Permission CRUD and search operations
/// </summary>
public interface IPermissionDataService : IDataService<Permission, CreatePermissionDTO, UpdatePermissionDTO, DeletePermissionDTO, InitPermissionDTO, PermissionDetailsDTO, PermissionSearchDTO>
{
}

/// <summary>
/// Data service for Permission entities with full CRUD, search, and validation support
/// </summary>
public class PermissionDataService : DataServiceBase<Permission, CreatePermissionDTO, UpdatePermissionDTO, DeletePermissionDTO, InitPermissionDTO, PermissionDetailsDTO, PermissionSearchDTO>, IPermissionDataService
{
    public PermissionDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Permission> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Permission, CreatePermissionDTO, UpdatePermissionDTO, DeletePermissionDTO, InitPermissionDTO, PermissionDetailsDTO, PermissionSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}