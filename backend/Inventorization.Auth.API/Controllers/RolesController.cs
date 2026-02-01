using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using InventorySystem.API.Base.Controllers;
using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.Role;
using Inventorization.Auth.Domain.DataServices;

namespace Inventorization.Auth.API.Controllers;

/// <summary>
/// Role management endpoints (CRUD operations)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController 
    : DataController<Role, CreateRoleDTO, UpdateRoleDTO, DeleteRoleDTO, RoleDetailsDTO, RoleSearchDTO, IRoleDataService>
{
    public RolesController(IRoleDataService service, ILogger<ServiceController> logger) 
        : base(service, logger)
    {
    }

    // All CRUD methods inherited from DataController:
    // GET /api/roles/{id} - GetByIdAsync
    // POST /api/roles - CreateAsync
    // PUT /api/roles/{id} - UpdateAsync
    // DELETE /api/roles/{id} - DeleteAsync
    // POST /api/roles/search - SearchAsync
}
