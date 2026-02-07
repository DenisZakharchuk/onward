using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.DTOs;
using Inventorization.Auth.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Inventorization.Auth.Domain.DataServices;

/// <summary>
/// Manages User-Role relationships with add/remove semantics
/// </summary>
public class UserRoleRelationshipManager : IRelationshipManager<User, Role>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserRoleRelationshipManager> _logger;

    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RelationshipUpdateResult> UpdateRelationshipsAsync(
        Guid userId,
        EntityReferencesDTO changes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate user exists
            var userExists = await _userRepository.ExistsAsync(userId, cancellationToken);
            if (!userExists)
                return RelationshipUpdateResult.Failure($"User {userId} not found");

            // Validate changes
            var validator = _serviceProvider.GetRequiredService<IValidator<EntityReferencesDTO>>();
            var validationResult = await validator.ValidateAsync(changes, cancellationToken);

            if (!validationResult.IsValid)
                return RelationshipUpdateResult.Failure("Validation failed", validationResult.Errors);

            int addedCount = 0;
            int removedCount = 0;

            // Remove relationships
            if (changes.IdsToRemove != null && changes.IdsToRemove.Any())
            {
                var existingRelationships = await _userRoleRepository.FindAsync(
                    ur => ur.UserId == userId && changes.IdsToRemove.Contains(ur.RoleId),
                    cancellationToken);

                foreach (var relationship in existingRelationships)
                {
                    await _userRoleRepository.DeleteAsync(relationship.Id, cancellationToken);
                    removedCount++;
                }

                _logger.LogInformation(
                    "Removed {Count} role relationships for User {UserId}",
                    removedCount,
                    userId);
            }

            // Add relationships
            if (changes.IdsToAdd != null && changes.IdsToAdd.Any())
            {
                // Check for duplicate relationships
                var existingRelationshipIds = (await _userRoleRepository.FindAsync(
                    ur => ur.UserId == userId && changes.IdsToAdd.Contains(ur.RoleId),
                    cancellationToken))
                    .Select(ur => ur.RoleId)
                    .ToHashSet();

                foreach (var roleId in changes.IdsToAdd)
                {
                    if (!existingRelationshipIds.Contains(roleId))
                    {
                        var userRole = new UserRole(userId, roleId);
                        await _userRoleRepository.CreateAsync(userRole, cancellationToken);
                        addedCount++;
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Skipped duplicate role {RoleId} for User {UserId}",
                            roleId,
                            userId);
                    }
                }

                _logger.LogInformation(
                    "Added {Count} role relationships for User {UserId}",
                    addedCount,
                    userId);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RelationshipUpdateResult.Success(addedCount, removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating User-Role relationships for User {UserId}", userId);
            return RelationshipUpdateResult.Failure($"Failed to update relationships: {ex.Message}");
        }
    }

    public async Task<BulkRelationshipUpdateResult> UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<Guid, RelationshipUpdateResult>();
        int totalAdded = 0;
        int totalRemoved = 0;
        int successful = 0;
        int failed = 0;
        var errors = new List<string>();

        _logger.LogInformation(
            "Starting bulk update of User-Role relationships for {Count} users",
            changes.Count);

        foreach (var (userId, entityChanges) in changes)
        {
            try
            {
                var result = await UpdateRelationshipsAsync(userId, entityChanges, cancellationToken);
                results[userId] = result;

                if (result.IsSuccess)
                {
                    totalAdded += result.AddedCount;
                    totalRemoved += result.RemovedCount;
                    successful++;
                }
                else
                {
                    failed++;
                    errors.AddRange(result.Errors.Select(e => $"User {userId}: {e}"));
                }
            }
            catch (Exception ex)
            {
                failed++;
                var errorMsg = $"User {userId}: {ex.Message}";
                errors.Add(errorMsg);
                results[userId] = RelationshipUpdateResult.Failure(ex.Message);

                _logger.LogError(ex, "Error in bulk update for User {UserId}", userId);
            }
        }

        _logger.LogInformation(
            "Bulk update completed: {Successful} successful, {Failed} failed, +{Added} -{Removed}",
            successful,
            failed,
            totalAdded,
            totalRemoved);

        if (failed == 0)
            return BulkRelationshipUpdateResult.Success(totalAdded, totalRemoved, successful, results);
        else
            return BulkRelationshipUpdateResult.PartialSuccess(totalAdded, totalRemoved, successful, failed, results, errors);
    }

    public async Task<List<Guid>> GetRelatedIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var relationships = await _userRoleRepository.FindAsync(
            ur => ur.UserId == userId,
            cancellationToken);

        return relationships.Select(ur => ur.RoleId).ToList();
    }
}
