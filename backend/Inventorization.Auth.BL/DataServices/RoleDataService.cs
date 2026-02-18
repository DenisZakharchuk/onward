using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Microsoft.Extensions.Logging;

namespace Inventorization.Auth.BL.DataServices;

/// <summary>
/// Data service interface for Role CRUD and search operations
/// </summary>
public interface IRoleDataService : IDataService<Role, CreateRoleDTO, UpdateRoleDTO, DeleteRoleDTO, InitRoleDTO, RoleDetailsDTO, RoleSearchDTO>
{
}

/// <summary>
/// Data service for Role entities with full CRUD, search, and validation support
/// </summary>
public class RoleDataService : DataServiceBase<Role, CreateRoleDTO, UpdateRoleDTO, DeleteRoleDTO, InitRoleDTO, RoleDetailsDTO, RoleSearchDTO>, IRoleDataService
{
    public RoleDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Role> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Role, CreateRoleDTO, UpdateRoleDTO, DeleteRoleDTO, InitRoleDTO, RoleDetailsDTO, RoleSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
