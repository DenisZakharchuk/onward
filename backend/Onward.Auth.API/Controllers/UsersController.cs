using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Onward.Base.API.Controllers;
using Onward.Auth.BL.Entities;
using Onward.Auth.DTO.DTO.Auth;
using Onward.Auth.DTO.DTO.User;
using Onward.Auth.BL.DataServices;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Base.DTOs;
using Onward.Base.Abstractions;

namespace Onward.Auth.API.Controllers;

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
    private readonly IUserAdminService _userAdminService;

    public UsersController(
        IUserDataService service,
        IRelationshipManager<User, Role> roleRelationshipManager,
        IUserAdminService userAdminService,
        ILogger<UsersController> logger) 
        : base(service, logger)
    {
        _roleHandler = new UserRoleRelationHandler(roleRelationshipManager, logger);
        _userAdminService = userAdminService;
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

    // ── Block / Unblock ────────────────────────────────────────────────────────

    /// <summary>
    /// Blocks a user account, deactivates it, and revokes all active refresh tokens.
    /// Any subsequent introspection of JTIs issued to this user will return inactive.
    /// </summary>
    [HttpPatch("{id}/block")]
    [ProducesResponseType(typeof(ServiceResult<bool>), 200)]
    [ProducesResponseType(typeof(ServiceResult<bool>), 400)]
    [ProducesResponseType(typeof(ServiceResult<bool>), 404)]
    public async Task<ActionResult<ServiceResult<bool>>> BlockUser(
        Guid id,
        [FromBody] BlockUserDTO request,
        CancellationToken cancellationToken)
    {
        var result = await _userAdminService.BlockUserAsync(id, request.Reason, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Unblocks a previously blocked user account. The user can log in again immediately.
    /// </summary>
    [HttpDelete("{id}/block")]
    [ProducesResponseType(typeof(ServiceResult<bool>), 200)]
    [ProducesResponseType(typeof(ServiceResult<bool>), 400)]
    [ProducesResponseType(typeof(ServiceResult<bool>), 404)]
    public async Task<ActionResult<ServiceResult<bool>>> UnblockUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _userAdminService.UnblockUserAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // Nested handler class for role relationships
    private class UserRoleRelationHandler : DataRelationHandler<User, Role>
    {
        public UserRoleRelationHandler(IRelationshipManager<User, Role> manager, ILogger logger)
            : base(manager, logger) { }
    }
}
