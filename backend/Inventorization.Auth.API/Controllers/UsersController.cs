using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using InventorySystem.API.Base.Controllers;
using Inventorization.Auth.BL.Entities;
using Inventorization.Auth.DTO.DTO.User;
using Inventorization.Auth.BL.DataServices;
using Inventorization.Base.DTOs;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.API.Controllers;

/// <summary>
/// User management endpoints (CRUD operations and role relationships)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController 
    : DataController<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, InitUserDTO, UserDetailsDTO, UserSearchDTO, IUserDataService>,
      IRelationController<Role>
{
    private readonly UserRoleRelationHandler _roleHandler;

    public UsersController(
        IUserDataService service,
        IRelationshipManager<User, Role> roleRelationshipManager,
        ILogger<UsersController> logger) 
        : base(service, logger)
    {
        _roleHandler = new UserRoleRelationHandler(roleRelationshipManager, logger);
    }

    // All CRUD methods inherited from DataController:
    // GET /api/users/{id} - GetByIdAsync
    // POST /api/users - CreateAsync
    // PUT /api/users/{id} - UpdateAsync
    // DELETE /api/users/{id} - DeleteAsync
    // POST /api/users/search - SearchAsync (via SearchController base)

    // IRelationController<Role> implementation

    /// <summary>
    /// Updates role assignments for a specific user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="changes">Role IDs to add and/or remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with counts of added/removed relationships</returns>
    [HttpPatch("{id}/relationships/roles")]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 200)]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 400)]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 500)]
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Role>.UpdateRelationshipsAsync(
        Guid id,
        [FromBody] EntityReferencesDTO changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateRelationshipsAsync(id, changes, "Roles", cancellationToken);
    }

    /// <summary>
    /// Bulk update role assignments for multiple users
    /// </summary>
    /// <param name="changes">Dictionary mapping user IDs to their role changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated result for all operations</returns>
    [HttpPatch("relationships/roles/bulk")]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 200)]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 400)]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 500)]
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> IRelationController<Role>.UpdateMultipleRelationshipsAsync(
        [FromBody] Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateMultipleRelationshipsAsync(changes, "Roles", cancellationToken);
    }

    // Nested handler class for role relationships
    private class UserRoleRelationHandler : DataRelationHandler<User, Role>
    {
        public UserRoleRelationHandler(IRelationshipManager<User, Role> manager, ILogger logger)
            : base(manager, logger) { }
    }
}

