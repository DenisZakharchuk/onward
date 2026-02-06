# Relationship Management - Future Considerations

This document tracks architectural considerations for the relationship management system that may be implemented in future iterations.

## 1. Query Performance for Bulk Operations

**Context**: `UpdateMultipleRelationshipsAsync()` with 1000+ entities could cause N+1 queries.

**Considerations**:
- Should architecture recommend batching strategy (chunks of 100)?
- Use `EF.CompileAsyncQuery()` for hot paths
- Implement batch size limits to prevent memory issues

**Recommendation**: Add performance guidance to Architecture.md - suggest compiled queries for frequently-used relationship operations and document recommended batch sizes (100-500 entities per transaction).

**Priority**: Medium - implement when bulk operations become common

---

## 2. Optimistic Concurrency for Relationships

**Context**: If two users simultaneously add/remove same relationship, last write wins.

**Considerations**:
- Should junction entities have `RowVersion` for concurrency detection?
- How to handle concurrent updates gracefully (retry, fail, merge)?
- Performance impact of row-level locking

**Recommendation**: Document as optional Tier 3 enhancement - add `RowVersion` to junction entities when concurrent updates are business-critical (e.g., payment allocations, inventory reservations).

**Priority**: Low - implement only for business-critical scenarios

**Example Implementation**:
```csharp
public class UserRole : ISoftDeletableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    
    [Timestamp]
    public byte[] RowVersion { get; private set; }
    
    // ... rest of implementation
}
```

---

## 3. Relationship History/Audit Trail

**Context**: Track who added/removed relationships and when (beyond soft delete).

**Considerations**:
- Integration with existing MongoDB audit logging system
- Should junction entities track `CreatedBy`, `CreatedAt`, `DeletedBy`, `DeletedAt`?
- Query performance impact of additional columns
- Compliance requirements (GDPR, audit regulations)

**Recommendation**: Reference existing audit logging in architecture doc - relationship changes already logged via `IAuditLogger` integration in DataServiceBase. For compliance-critical relationships, upgrade to Tier 3 (full CRUD) with audit columns.

**Priority**: Low - existing MongoDB audit system sufficient for most scenarios

**Current Behavior**:
- All CRUD operations automatically logged to MongoDB via `IAuditLogger`
- Includes: action, entityType, entityId, userId, timestamp, changes
- 90-day TTL with automatic cleanup
- GraphQL queries available via `/graphql` endpoint

---

## 4. Relationship Validation Rules Engine

**Context**: Complex business rules for relationships (e.g., "User can have max 5 roles", "Cannot assign conflicting permissions").

**Considerations**:
- Centralized rules engine vs validation in each `IRelationshipManager`
- Rule definition format (code, configuration, database)
- Performance impact of rule evaluation

**Recommendation**: Start with validation in `IValidator<EntityReferencesDTO>` implementations. If rules become complex, consider extracting to dedicated `IRelationshipRuleEngine<TEntity, TRelatedEntity>`.

**Priority**: Low - implement when business rules become complex

---

## 5. Relationship Caching Strategy

**Context**: Frequently-accessed relationships (e.g., user roles, permissions) could benefit from caching.

**Considerations**:
- Cache invalidation on relationship changes
- Distributed cache (Redis) vs in-memory cache
- Cache key structure: `{EntityType}:{EntityId}:relationships:{RelatedEntityType}`
- TTL strategy

**Recommendation**: Add caching layer in `IRelationshipManager` implementations when performance profiling identifies hot paths. Use decorator pattern to avoid coupling core logic to cache.

**Priority**: Low - implement only after performance metrics justify it

---

## 6. Graph-Based Relationship Queries

**Context**: Complex relationship queries (e.g., "Find all users with access to resource X through any path").

**Considerations**:
- Should we support graph traversal queries?
- Integration with graph databases (Neo4j) for complex hierarchies
- SQL recursive CTEs for tree structures

**Recommendation**: Start with EF Core navigation properties and `Include()`. Consider graph database only for highly-connected domains (social networks, org charts, knowledge graphs).

**Priority**: Very Low - implement only for graph-heavy domains

---

## 7. Relationship Change Notifications

**Context**: Notify users when relationships change (e.g., "You've been added to project X").

**Considerations**:
- Integration with message broker (existing system has message broker in docker-compose)
- Event publishing: `RelationshipAdded`, `RelationshipRemoved` events
- SignalR for real-time updates to connected clients

**Recommendation**: Publish domain events from `IRelationshipManager` implementations. Event handlers can trigger notifications, webhooks, or other side effects.

**Priority**: Medium - common requirement for collaborative systems

---

## 8. Relationship Import/Export

**Context**: Bulk operations for migrating data or integrating external systems.

**Considerations**:
- CSV/Excel import format: `EntityId,RelatedEntityIds`
- Validation during import (all entities exist, no duplicates)
- Rollback strategy for failed imports
- Progress reporting for large imports (1000+ relationships)

**Recommendation**: Add specialized import/export endpoints when bulk operations become common. Use background jobs for large datasets (Hangfire, Quartz.NET).

**Priority**: Low - implement when integration requirements emerge

---

## Review Schedule

- **Next Review**: After 3 months of production usage
- **Metrics to Track**:
  - Bulk operation usage frequency and size
  - Relationship query performance (P95, P99 latency)
  - Concurrent update conflicts frequency
  - Audit query usage patterns
