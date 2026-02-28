using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Permission;
using Onward.Base.Abstractions;
using Onward.Base.DataAccess;
using Onward.Base.Services;
using Microsoft.Extensions.Logging;

namespace Onward.Auth.BL.DataServices;

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
        Onward.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Permission> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Permission, CreatePermissionDTO, UpdatePermissionDTO, DeletePermissionDTO, InitPermissionDTO, PermissionDetailsDTO, PermissionSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}