using InventorySystem.Business.Abstractions;
using InventorySystem.AuditLog.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Text.Json;

namespace InventorySystem.AuditLog.Services;

public class MongoAuditLogger : IAuditLogger
{
    private readonly IMongoCollection<AuditEntry> _collection;
    private readonly ILogger<MongoAuditLogger> _logger;
    private const int MaxChangesSizeBytes = 15 * 1024 * 1024; // 15MB to be safe under 16MB limit

    public MongoAuditLogger(IConfiguration configuration, ILogger<MongoAuditLogger> logger)
    {
        _logger = logger;

        var connectionString = configuration["MongoDB:ConnectionString"] 
            ?? "mongodb://admin:admin123@localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "inventorydb";
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<AuditEntry>("auditLogs");

        // Create TTL index for 90-day retention
        CreateTtlIndexAsync().Wait();
    }

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = new AuditEntry
            {
                UserId = entry.UserId,
                Action = entry.Action,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Changes = TruncateIfNeeded(entry.Changes),
                Metadata = entry.Metadata,
                IpAddress = entry.IpAddress,
                Timestamp = DateTime.UtcNow
            };

            await _collection.InsertOneAsync(auditEntry, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Never throw - audit logging should not break business operations
            _logger.LogError(ex, "Failed to write audit log for {EntityType} {EntityId}", 
                entry.EntityType, entry.EntityId);
        }
    }

    private Dictionary<string, object> TruncateIfNeeded(Dictionary<string, object> changes)
    {
        try
        {
            var json = JsonSerializer.Serialize(changes);
            var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(json);

            if (sizeBytes > MaxChangesSizeBytes)
            {
                _logger.LogWarning("Audit log changes exceed size limit ({Size} bytes), truncating", sizeBytes);
                return new Dictionary<string, object>
                {
                    { "_truncated", true },
                    { "_originalSize", sizeBytes },
                    { "_message", "Changes too large to store" }
                };
            }

            return changes;
        }
        catch
        {
            return changes; // If serialization fails, return as-is
        }
    }

    private async Task CreateTtlIndexAsync()
    {
        try
        {
            var indexKeys = Builders<AuditEntry>.IndexKeys.Ascending(x => x.Timestamp);
            var indexOptions = new CreateIndexOptions 
            { 
                ExpireAfter = TimeSpan.FromDays(90) 
            };
            var indexModel = new CreateIndexModel<AuditEntry>(indexKeys, indexOptions);
            await _collection.Indexes.CreateOneAsync(indexModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create TTL index on audit logs");
        }
    }
}
