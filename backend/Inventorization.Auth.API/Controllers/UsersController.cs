using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using InventorySystem.API.Base.Controllers;
using Inventorization.Auth.Domain.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Auth.Domain.DataServices;

namespace Inventorization.Auth.API.Controllers;

/// <summary>
/// User management endpoints (CRUD operations)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController 
    : DataController<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, UserDetailsDTO, UserSearchDTO, IUserDataService>
{
    public UsersController(IUserDataService service, ILogger<ServiceController> logger) 
        : base(service, logger)
    {
    }

    // All CRUD methods inherited from DataController:
    // GET /api/users/{id} - GetByIdAsync
    // POST /api/users - CreateAsync
    // PUT /api/users/{id} - UpdateAsync
    // DELETE /api/users/{id} - DeleteAsync
    // POST /api/users/search - SearchAsync (via SearchController base)
}
