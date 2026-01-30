using InventorySystem.AuditLog.Abstractions;
using InventorySystem.AuditLog.Models;

namespace InventorySystem.API.GraphQL;

public class AuditLogQuery
{
    public async Task<IEnumerable<AuditEntry>> GetAuditLogs(
        [Service] IAuditRepository repository,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? entityType = null,
        string? action = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (fromDate == null && toDate == null && entityType == null && action == null && userId == null)
        {
            return await repository.GetAllAsync(cancellationToken);
        }

        return await repository.GetFilteredAsync(fromDate, toDate, entityType, action, userId, cancellationToken);
    }

    public async Task<AuditEntry?> GetAuditLogById(
        [Service] IAuditRepository repository,
        string id,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetByIdAsync(id, cancellationToken);
    }
}
