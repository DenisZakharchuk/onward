using InventorySystem.AuditLog.Abstractions;
using InventorySystem.AuditLog.Models;
using MongoDB.Driver;

namespace InventorySystem.AuditLog.Services;

public class MongoAuditRepository : IAuditRepository
{
    private readonly IMongoCollection<AuditEntry> _collection;

    public MongoAuditRepository(IMongoClient mongoClient, string databaseName = "inventorydb")
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<AuditEntry>("auditLogs");
    }

    public async Task<IEnumerable<AuditEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true)
            .Sort(Builders<AuditEntry>.Sort.Descending(x => x.Timestamp))
            .Limit(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditEntry?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditEntry>> GetFilteredAsync(
        DateTime? fromDate,
        DateTime? toDate,
        string? entityType,
        string? action,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<AuditEntry>.Filter;
        var filters = new List<FilterDefinition<AuditEntry>>();

        if (fromDate.HasValue)
            filters.Add(filterBuilder.Gte(x => x.Timestamp, fromDate.Value));

        if (toDate.HasValue)
            filters.Add(filterBuilder.Lte(x => x.Timestamp, toDate.Value));

        if (!string.IsNullOrEmpty(entityType))
            filters.Add(filterBuilder.Eq(x => x.EntityType, entityType));

        if (!string.IsNullOrEmpty(action))
            filters.Add(filterBuilder.Eq(x => x.Action, action));

        if (!string.IsNullOrEmpty(userId))
            filters.Add(filterBuilder.Eq(x => x.UserId, userId));

        var filter = filters.Any()
            ? filterBuilder.And(filters)
            : filterBuilder.Empty;

        return await _collection.Find(filter)
            .Sort(Builders<AuditEntry>.Sort.Descending(x => x.Timestamp))
            .Limit(500)
            .ToListAsync(cancellationToken);
    }
}
