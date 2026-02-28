using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Onward.Base.API.Controllers;
using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Permission;
using Onward.Auth.BL.DataServices;

namespace Onward.Auth.API.Controllers;

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
