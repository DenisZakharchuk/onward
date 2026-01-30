namespace InventorySystem.Business.Abstractions;

/// <summary>
/// Abstraction for audit logging. Business layer depends on this interface only.
/// Implementation can be MongoDB, message broker, file system, etc.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log an audit entry asynchronously (fire-and-forget pattern).
    /// Should not throw exceptions to avoid breaking business operations.
    /// </summary>
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

public class AuditLogEntry
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string UserId { get; set; } = "system";
    public string IpAddress { get; set; } = string.Empty;
    public Dictionary<string, object> Changes { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
