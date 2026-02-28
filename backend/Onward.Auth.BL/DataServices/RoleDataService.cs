using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Role;
using Onward.Base.Abstractions;
using Onward.Base.DataAccess;
using Onward.Base.Services;
using Microsoft.Extensions.Logging;

namespace Onward.Auth.BL.DataServices;

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
        Onward.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Role> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Role, CreateRoleDTO, UpdateRoleDTO, DeleteRoleDTO, InitRoleDTO, RoleDetailsDTO, RoleSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
}
