using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using InventorySystem.API.Base.Controllers;
using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.Permission;
using Inventorization.Auth.BL.DataServices;

namespace Inventorization.Auth.API.Controllers;

/// <summary>
/// Permission management endpoints (CRUD operations)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController 
    : DataController<Permission, CreatePermissionDTO, UpdatePermissionDTO, DeletePermissionDTO, InitPermissionDTO, PermissionDetailsDTO, PermissionSearchDTO, IPermissionDataService>
{
    public PermissionsController(IPermissionDataService service, ILogger<ServiceController> logger) 
        : base(service, logger)
    {
    }

    // All CRUD methods inherited from DataController:
    // GET /api/permissions/{id} - GetByIdAsync
    // POST /api/permissions - CreateAsync
    // PUT /api/permissions/{id} - UpdateAsync
    // DELETE /api/permissions/{id} - DeleteAsync
    // POST /api/permissions/search - SearchAsync
}
