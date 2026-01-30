using InventorySystem.AuditLog.Models;

namespace InventorySystem.AuditLog.Abstractions;

public interface IAuditRepository
{
    Task<IEnumerable<AuditEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AuditEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditEntry>> GetFilteredAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? entityType,
        string? action,
        string? userId,
        CancellationToken cancellationToken = default);
}
