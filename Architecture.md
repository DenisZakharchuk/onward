# Inventorization dashboard
 Project consist of multiple layers:
  - Frontend: vue 3 application
  - Backend: multiple .net8 asp.net microservices, they use entity framework to interract with sql db - postgresql
  - DB: postgresql, containerized (docker), MongoDB server
  - message broker

# Frontend
 It's regular dashboard application. 
 It has authorization user story. For now it has local authorization (using AuthService microservice).
 It has set of list views and form views for entities of the DataModel;
 Business loic layer is implemented in separate abstractions in "services" folder. They are injected where they needed with provide-use pattern
 
# Backend
 Microservice arhiteture. Each microservice has 
 - separate, dedicated DB (if it's needed). 
 - restfull API implemented in controllers
 - API can require authorization (using bearer JWT auth method), or allow anonymous authorization (for example for AuthoService)
 - swagger
 - business logic layer implemented in separate project
 We need to have separate microservice (with dedicated Data Storage) for each set of interconnected entities
 Naming conventions for projects: 
 - DTO projects: Inventorization.[BoundedContextName].DTO; class library;
 - Inventorization.[BoundedContextName].BL; class library; It has folders: Entities, Services, DbContexts, UOWs. Inventorization.[BoundedContextName].DTO is a dependency;
 - Inventorization.[BoundedContextName].API; asp.net web app; Inventorization.[BoundedContextName].BL and Inventorization.[BoundedContextName].DTO are dependencies
 all microservices are containarized;
 maintein docker-compose file in workspace root directory for all microservices

# Backend Architecture Rules & Patterns
## Entity Immutability Pattern
All entity classes must follow strict immutability principles to ensure data consistency and prevent accidental state modifications:

### Entity Implementation Rules
1. **Property Setters**: All properties must have **private setters** to prevent direct assignment from outside the entity
   ```csharp
   public string Email { get; private set; } = null!;
   ```

2. **Parameterless Constructor**: Required for EF Core but must be **private** to prevent instantiation without proper initialization
   ```csharp
   private User() { }  // EF Core only
   ```

3. **Parameterized Constructor**: Robust constructor accepting all required properties, validating inputs
   ```csharp
   public User(string email, string passwordHash, string fullName)
   {
       if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required");
       Email = email;
       PasswordHash = passwordHash;
       FullName = fullName;
   }
   ```

4. **State Mutations**: All entity state changes must go through dedicated methods, never direct property assignment
   ```csharp
   public void UpdateProfile(string fullName)
   {
       if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name required");
       FullName = fullName;
       UpdatedAt = DateTime.UtcNow;
   }
   
   public void Deactivate()
   {
       IsActive = false;
       UpdatedAt = DateTime.UtcNow;
   }
   ```

5. **Navigation Properties**: Keep public getters for EF Core lazy loading, but collections use `[JsonIgnore]` if returned in APIs
   ```csharp
   public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
   ```

6. **Computed/Read-Only Properties**: Allowed for derived values
   ```csharp
   public bool IsValid => DateTime.UtcNow < ExpiryDate && RevokedAt == null;
   ```

### Example Entity
```csharp
public class User
{
    private User() { }  // EF Core only, private to prevent misuse
    
    public User(string email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash required");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name required");
        
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public ICollection<UserRole> UserRoles { get; } = new List<UserRole>();
    
    public void UpdateProfile(string email, string fullName)
    {
        Email = email;
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash required");
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Relationship Mutation Methods

Entities can have domain methods for managing relationships in addition to API-level endpoints. This hybrid approach allows:
- **Domain Methods**: Used within the application for in-process relationship management
- **API Endpoints**: Used by external clients via HTTP

**Rules for Entity Relationship Methods:**
1. Methods validate business rules before modifying relationships
2. Methods modify navigation collections directly
3. Methods **DO NOT** call `SaveChangesAsync()` - caller's responsibility
4. Methods throw exceptions for invalid operations

**Example: User entity with role management**
```csharp
public class User
{
    private User() { }  // EF Core only
    
    public User(string email, string passwordHash, string fullName)
    {
        // ... constructor logic
    }
    
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    
    // Relationship mutation methods
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));
        
        if (HasRole(role.Id))
            throw new InvalidOperationException($"User already has role {role.Name}");
        
        UserRoles.Add(new UserRole(Id, role.Id));
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void RevokeRole(Guid roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole == null)
            throw new InvalidOperationException($"User does not have role {roleId}");
        
        UserRoles.Remove(userRole);
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool HasRole(Guid roleId) => UserRoles.Any(ur => ur.RoleId == roleId);
    
    // Other entity methods...
}
```

**When to add relationship methods to entities:**
- Simple associations (one-to-many, many-to-many via junction)
- Business logic requires validation before relationship changes
- Domain services need to manage relationships in-process
- Relationships are part of the entity's core domain behavior

**When to skip entity methods:**
- Relationships have no business validation rules
- Relationships managed purely through CRUD operations
- Junction entity has additional metadata requiring full service layer

---

## Ownership System

Entities can be *owned* by users or tenants. The ownership system is implemented as a small, strongly-typed value-object hierarchy with no direct coupling to the Identity bounded context. All types live in `Inventorization.Base/Ownership/` and `Inventorization.Base.AspNetCore/`.

### Ownership Value Objects

All ownership VOs inherit from the abstract base record:

```csharp
// Inventorization.Base/Ownership/OwnershipValueObject.cs
public abstract record OwnershipValueObject;

// Concrete variants:
public record UserOwnership(Guid UserId) : OwnershipValueObject;
public record UserTenantOwnership(Guid UserId, Guid TenantId) : OwnershipValueObject;
```

**Rule**: Use `UserTenantOwnership` as the default for all multi-tenant bounded contexts. Use `UserOwnership` only for single-tenant or personal contexts.

### Owned Entity Interfaces & Base Classes

```csharp
// IOwnedEntity<TOwnership>.cs
public interface IOwnedEntity<TOwnership> where TOwnership : OwnershipValueObject
{
    TOwnership? Ownership { get; }          // Creator's ownership stamp
    TOwnership? LastModifiedOwnership { get; }  // Last modifier's stamp
    void SetOwnership(TOwnership ownership);
    void UpdateOwnership(TOwnership ownership);
}

// OwnedBaseEntity<TOwnership, TPrimaryKey> — owned entity with custom PK
// OwnedBaseEntity<TOwnership> : OwnedBaseEntity<TOwnership, Guid>  — shortcut
public abstract class OwnedBaseEntity<TOwnership> : BaseEntity, IOwnedEntity<TOwnership>
    where TOwnership : OwnershipValueObject
{
    // Ownership properties added automatically
    public TOwnership? Ownership { get; private set; }
    public TOwnership? LastModifiedOwnership { get; private set; }

    public void SetOwnership(TOwnership ownership) => Ownership = ownership;
    public void UpdateOwnership(TOwnership ownership) => LastModifiedOwnership = ownership;
}
```

**Entity declaration**:
```csharp
// Non-owned entity (default)
public class Category : BaseEntity { ... }

// Owned entity (user + tenant scope)
public class Order : OwnedBaseEntity<UserTenantOwnership> { ... }
```

### Identity Context Abstractions

The bounded context layer knows **nothing** about JWT or HTTP. It depends only on `ICurrentIdentityContext<TOwnership>`:

```csharp
// Inventorization.Base/Abstractions/ICurrentIdentityContext.cs
public interface ICurrentIdentityContext<TOwnership>
    where TOwnership : OwnershipValueObject
{
    TOwnership? Ownership { get; }       // UserId/TenantId wrapped in VO
    string? Email { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

// Inventorization.Base/Abstractions/ICurrentUserService.cs
public interface ICurrentUserService<TOwnership>
    where TOwnership : OwnershipValueObject
{
    Task<bool> HasPermissionAsync(string resource, string action, CancellationToken ct = default);
    Task<bool> CanAccessEntityAsync<TEntity>(TEntity entity, CancellationToken ct = default)
        where TEntity : IOwnedEntity<TOwnership>;
}
```

**Anonymous fallback** (null-object, no registration needed): `AnonymousIdentityContext<TOwnership>.Instance`

### HTTP Layer — Inventorization.Base.AspNetCore

JWT → `ICurrentIdentityContext<TOwnership>` resolution lives in the separate project `Inventorization.Base.AspNetCore`:

| Class | Responsibility |
|---|---|
| `HttpContextCurrentIdentityContext<TOwnership>` | Reads `name` + `tenant_id` claims, delegates to `IOwnershipFactory` |
| `ClaimsCurrentUserService<TOwnership>` | `AnAdmin` role bypass; permission claim convention `resource.action` |
| `UserOwnershipFactory` | `(userId, _) => new UserOwnership(userId)` |
| `UserTenantOwnershipFactory` | `(userId, tenantId) => new UserTenantOwnership(userId, tenantId ?? Guid.Empty)` |

**DI registration** (in API `Program.cs`):
```csharp
// Convenience method — registers all 4 services in one call
builder.Services.AddUserTenantOwnershipServices(); // UserTenantOwnership + UserTenantOwnershipFactory
// or:
builder.Services.AddOwnershipServices<UserTenantOwnership, UserTenantOwnershipFactory>();
```

**Project references required:**
```
Inventorization.[BoundedContext].API  →  Inventorization.Base.AspNetCore
                                      →  Inventorization.Base
```

### Ownership in DataServiceBase

For owned entities use the 8-type-parameter variant that constructor-injects identity context:

```csharp
// Owned data service — DataServiceBase stamps Ownership on Add/Update
public class OrderDataService
    : DataServiceBase<UserTenantOwnership,
                      Order, CreateOrderDTO, UpdateOrderDTO, DeleteOrderDTO,
                      OrderDetailsDTO, OrderSearchDTO>,
      IOrderDataService
{
    public OrderDataService(
        IUnitOfWork unitOfWork,
        IRepository<Order> repository,
        ICurrentIdentityContext<UserTenantOwnership> identityContext,
        ILogger<OrderDataService> logger)
        : base(unitOfWork, repository, identityContext, logger) { }
}

// Non-owned (existing) — backward-compatible, no changes required
public class CategoryDataService
    : DataServiceBase<Category, CreateCategoryDTO, ...>, ICategoryDataService { ... }
```

`DataServiceBase<TOwnership, ...>` automatically:
- Calls `entity.SetOwnership(identityContext.Ownership)` inside `AddAsync`
- Calls `entity.UpdateOwnership(identityContext.Ownership)` inside `UpdateAsync`
- Returns a `403 Forbidden` `ServiceResult` when `!identityContext.IsAuthenticated`

### Ownership in Query Layer

Query filtering by ownership is integrated into `BaseQueryBuilder` and `BaseSearchService` via ownership-aware generic overloads — **not** via global EF Core query filters.

```csharp
// IQueryBuilder<TEntity, TOwnership> extends IQueryBuilder<TEntity>
public interface IQueryBuilder<TEntity, TOwnership> : IQueryBuilder<TEntity>
    where TEntity : class
    where TOwnership : OwnershipValueObject
{
    IQueryable<TEntity> BuildOwnedQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery);
}

// BaseQueryBuilder<TEntity, TOwnership> — constructor-injects ICurrentIdentityContext
public class OrderQueryBuilder : BaseQueryBuilder<Order, UserTenantOwnership>
{
    public OrderQueryBuilder(ICurrentIdentityContext<UserTenantOwnership> identityContext)
        : base(identityContext) { }
    // Override BuildOwnershipPredicate if non-default matching is needed
}

// BaseSearchService<TEntity, TProjection, TOwnership>
public class OrderSearchService
    : BaseSearchService<Order, OrderProjection, UserTenantOwnership>
{
    public OrderSearchService(
        IRepository<Order> repository,
        IQueryBuilder<Order, UserTenantOwnership> queryBuilder,
        IProjectionMapper<Order, OrderProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ICurrentIdentityContext<UserTenantOwnership> identityContext,
        ILogger<OrderSearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, identityContext, logger) { }
}
```

**`BuildOwnedQuery` behaviour**:
- Anonymous caller (`!IsAuthenticated`) → returns `Queryable.Empty<TEntity>()`
- Authenticated → calls `BuildQuery` then applies `BuildOwnershipPredicate(ownership)` (default: record equality on `Ownership` property)

---

## ADT-Based Query/Search Architecture

### Overview

The ADT-based (Algebraic Data Type) query architecture provides a type-safe, composable, and highly testable approach to querying entities. It replaces traditional query providers with a layered architecture built on immutable ADT structures.

**Key Benefits:**
- **Type Safety**: Compile-time validation of query structure
- **Composability**: Filters, projections, and transformations can be nested and combined
- **Testability**: Each layer independently testable with clear interfaces
- **Consistency**: All entities follow identical query pattern
- **Flexibility**: Virtual methods allow customization without breaking base behavior
- **Metadata-Driven**: Validators use DataModelMetadata for runtime type checking

**Philosophy**: Instead of building queries imperatively, clients construct declarative ADT structures (SearchQuery) that describe what they want. The backend translates these structures into LINQ expressions and SQL.

### Component Layers

The architecture follows a clean 4-layer pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│ Client (HTTP POST /api/{entity}/query)              │
│ Sends: SearchQuery ADT (JSON)                       │
└─────────────────┬───────────────────────────────────┘
                  │ HTTP POST with SearchQuery
┌─────────────────▼───────────────────────────────────┐
│ Layer 1: QueryController (BaseQueryController)      │
│ - Route handling ([ApiController])                   │
│ - Request validation                                 │
│ - Response wrapping (ServiceResult)                  │
│ Injects: ISearchService<TEntity, TProjection>       │
└─────────────────┬───────────────────────────────────┘
                  │ Delegates to service
┌─────────────────▼───────────────────────────────────┐
│ Layer 2: SearchService (BaseSearchService)          │
│ - Query validation orchestration                     │
│ - Query building orchestration                       │
│ - Projection application orchestration              │
│ - Error handling and logging                        │
│ Injects: IRepository, IQueryBuilder,                │
│          IProjectionMapper, IValidator, Logger       │
└─────────────────┬───────────────────────────────────┘
                  │ Uses multiple components
        ┌─────────┼─────────┬──────────┬──────────┐
        ▼         ▼         ▼          ▼          ▼
   IQueryBuilder  IProjection IValidator IRepository ProjectionExpression
   (Layer 3)      Mapper      (validate) (data)     Builder (transforms)
                  (Layer 4)
```

**Layer Responsibilities:**

1. **Controller Layer**: HTTP concerns only (routing, status codes, response wrapping)
2. **Service Layer**: Orchestration and business logic flow
3. **Query Builder Layer**: ADT → LINQ expression translation
4. **Infrastructure Layer**: Projection mapping, validation, data access

### Base Classes

#### BaseQueryController\<TEntity, TProjection\>

**Location**: `InventorySystem.API.Base/Controllers/BaseQueryController.cs`

**Purpose**: Provides standardized HTTP endpoints for ADT-based queries

**Generic Constraints:**
- `TEntity : class` - The entity being queried
- `TProjection : class, new()` - The DTO projection result type

**Dependencies (Constructor Injection):**
```csharp
ISearchService<TEntity, TProjection> searchService
ILogger logger
```

**Virtual Members:**
```csharp
protected virtual string EntityName => typeof(TEntity).Name;  // For logging
public virtual async Task<ActionResult<ServiceResult<SearchResult<TProjection>>>> Query(...)
public virtual async Task<ActionResult<ServiceResult<SearchResult<TransformationResult>>>> QueryWithTransformations(...)
```

**Endpoints:**
- `[HttpPost]` `/api/{entity}/query` → Regular search with projections
- `[HttpPost("transform")]` `/api/{entity}/query/transform` → Computed field transformations

**Concrete Implementation** (per entity, ~25 lines):
```csharp
[ApiController]
[Route("api/goods/query")]
[AllowAnonymous]
public class GoodsQueryController : BaseQueryController<Good, GoodProjection>
{
    public GoodsQueryController(
        ISearchService<Good, GoodProjection> searchService,
        ILogger<GoodsQueryController> logger)
        : base(searchService, logger) { }
}
```

#### BaseSearchService\<TEntity, TProjection\> / BaseSearchService\<TEntity, TProjection, TOwnership\>

**Location**: `Inventorization.Base/DataAccess/BaseSearchService.cs`

**Purpose**: Orchestrates query execution pipeline (validate → build → execute → project)

**Two variants:**
- `BaseSearchService<TEntity, TProjection>` — non-owned entities; no identity context
- `BaseSearchService<TEntity, TProjection, TOwnership>` — owned entities; adds `ICurrentIdentityContext<TOwnership>` and `IQueryBuilder<TEntity, TOwnership>`; exposes `ExecuteOwnedSearchAsync`

**Generic Constraints:**
- `TEntity : class`
- `TProjection : class, new()` - Required by IProjectionMapper
- `TOwnership : OwnershipValueObject` (ownership variant only)

**Dependencies (Constructor Injection):**
```csharp
// Non-owned variant
IRepository<TEntity> repository           // Data access
IQueryBuilder<TEntity> queryBuilder       // ADT → LINQ expression
IProjectionMapper<TEntity, TProjection> projectionMapper  // Entity → DTO
ProjectionExpressionBuilder expressionBuilder  // Field transformations
IValidator<SearchQuery> validator         // Query validation
ILogger logger                            // Logging

// Ownership variant adds:
IQueryBuilder<TEntity, TOwnership> ownedQueryBuilder  // ownership-scoped query builder
ICurrentIdentityContext<TOwnership> identityContext   // caller's ownership stamp
```

**Virtual Members:**
```csharp
protected virtual string EntityName => typeof(TEntity).Name;  // For logging
public virtual async Task<ServiceResult<SearchResult<TProjection>>> ExecuteSearchAsync(...)
public virtual async Task<ServiceResult<SearchResult<TransformationResult>>> ExecuteTransformationSearchAsync(...)
```

**Execution Flow:**
1. Validate SearchQuery using IValidator
2. Get base IQueryable from repository
3. Build filtered/sorted query using IQueryBuilder
4. Count total results (before pagination)
5. Apply pagination
6. Execute query with projection (or transformations)
7. Return SearchResult wrapped in ServiceResult

**Concrete Implementation** (per entity, ~28 lines):
```csharp
public class GoodSearchService : BaseSearchService<Good, GoodProjection>
{
    public GoodSearchService(
        IRepository<Good> repository,
        IQueryBuilder<Good> queryBuilder,
        IProjectionMapper<Good, GoodProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger<GoodSearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, logger) { }
}
```

#### BaseQueryBuilder\<TEntity\> / BaseQueryBuilder\<TEntity, TOwnership\>

**Location**: `Inventorization.Base/DataAccess/BaseQueryBuilder.cs`

**Purpose**: Converts SearchQuery ADT (filters, projections, sorting) to LINQ expressions

**Two variants:**
- `BaseQueryBuilder<TEntity>` — non-owned entities
- `BaseQueryBuilder<TEntity, TOwnership>` — owned entities; constructor-injects `ICurrentIdentityContext<TOwnership>`; implements `IQueryBuilder<TEntity, TOwnership>`

**Generic Constraints:**
- `TEntity : class`
- `TOwnership : OwnershipValueObject` (ownership variant only)

**Virtual Members:**
```csharp
protected virtual string ParameterName => typeof(TEntity).Name.ToLower()[0].ToString();
// Ownership variant also exposes:
protected virtual Expression<Func<TEntity, bool>> BuildOwnershipPredicate(TOwnership ownership)
// default: record equality on entity.Ownership property
```

**Key Methods:**
```csharp
Expression<Func<TEntity, bool>>? BuildFilterExpression(FilterExpression? filter)
IQueryable<TEntity> ApplyProjection(IQueryable<TEntity> query, ProjectionRequest? projection)
IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, SortRequest? sort)
IQueryable<TEntity> BuildQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery)
// Ownership variant adds:
IQueryable<TEntity> BuildOwnedQuery(IQueryable<TEntity> baseQuery, SearchQuery searchQuery)
```

**Pattern Matching**: Recursively processes FilterExpression ADT variants:
- `LeafFilter` → Single condition (Equals, GreaterThan, Contains, etc.)
- `AndFilter` → Logical AND of child filters
- `OrFilter` → Logical OR of child filters

**Concrete Implementation** (per entity):
```csharp
// Non-owned (~13 lines)
public class GoodQueryBuilder : BaseQueryBuilder<Good>
{
    // Empty body - uses all defaults from base class
    // Override ParameterName only if custom name needed
}

// Owned (~15 lines) — requires ICurrentIdentityContext injected from DI
public class OrderQueryBuilder : BaseQueryBuilder<Order, UserTenantOwnership>
{
    public OrderQueryBuilder(ICurrentIdentityContext<UserTenantOwnership> ctx)
        : base(ctx) { }
    // Override BuildOwnershipPredicate only for non-standard ownership matching
}
```

#### ProjectionMapperBase\<TEntity, TProjection\>

**Location**: `Inventorization.Base/Abstractions/ProjectionMapperBase.cs`

**Purpose**: Maps entities to DTOs using EF Core expressions or in-memory mapping

**Template Method Pattern** - Base class provides:
```csharp
public virtual Expression<Func<TEntity, TProjection>> GetProjectionExpression(ProjectionRequest? projection)
public virtual TProjection Map(TEntity entity, ProjectionRequest? projection, int currentDepth = 0)
```

**Abstract Methods** (must be implemented by concrete classes):
```csharp
protected abstract Expression<Func<TEntity, TProjection>> GetAllFieldsProjection(bool deep, int depth);
protected abstract Expression<Func<TEntity, TProjection>> BuildSelectiveProjection(ProjectionRequest projection);
protected abstract void MapAllFields(TEntity entity, TProjection result, bool deep, int maxDepth, int currentDepth);
protected abstract void MapField(TEntity entity, TProjection result, string fieldName, int maxDepth, int currentDepth);
```

**Concrete Implementation** (per entity, ~250 lines - most complex infrastructure component):
```csharp
public class GoodProjectionMapper : ProjectionMapperBase<Good, GoodProjection>, IGoodProjectionMapper
{
    private readonly ICategoryProjectionMapper _categoryMapper;  // For nested projections
    
    protected override Expression<Func<Good, GoodProjection>> GetAllFieldsProjection(bool deep, int depth)
    {
        return g => new GoodProjection
        {
            Id = g.Id,
            Name = g.Name,
            // ... all properties
            Category = deep && depth > 0 && g.Category != null 
                ? new CategoryProjection { /* nested fields */ } 
                : null
        };
    }
    
    protected override Expression<Func<Good, GoodProjection>> BuildSelectiveProjection(ProjectionRequest projection)
    {
        // HashSet for O(1) field lookups (evaluated outside expression tree)
        var requestedFields = new HashSet<string>(
            projection.Fields.Select(f => f.FieldName), 
            StringComparer.OrdinalIgnoreCase);
        
        var hasName = requestedFields.Contains("Name");
        var hasCategory = requestedFields.Contains("Category.Name");
        
        // Use constants in expression tree for EF Core compatibility
        return g => new GoodProjection
        {
            Name = hasName ? g.Name : null,
            Category = hasCategory ? new CategoryProjection { Name = g.Category.Name } : null
        };
    }
    
    protected override void MapAllFields(Good entity, GoodProjection result, 
        bool deep, int maxDepth, int currentDepth)
    {
        result.Name = entity.Name;
        // ... all properties
        
        if (deep && currentDepth < maxDepth && entity.Category != null)
        {
            var categoryProjection = ProjectionRequest.AllDeep(maxDepth - currentDepth - 1);
            result.Category = _categoryMapper.Map(entity.Category, categoryProjection, currentDepth + 1);
        }
    }
    
    protected override void MapField(Good entity, GoodProjection result, string fieldName, 
        int maxDepth, int currentDepth)
    {
        switch (fieldName.ToLower())
        {
            case "name": result.Name = entity.Name; break;
            case "category.name":
                if (entity.Category != null)
                    result.Category = new GoodProjection { Name = entity.Category.Name };
                break;
        }
    }
}
```

### ADT Types Reference

All ADT classes located in `Inventorization.Base/ADTs/`

#### SearchQuery

**Purpose**: Top-level query structure combining all query aspects

```csharp
public sealed record SearchQuery
{
    public FilterExpression? Filter { get; init; }        // WHERE clause
    public ProjectionRequest? Projection { get; init; }    // SELECT fields
    public SortRequest? Sort { get; init; }                // ORDER BY
    public PageRequest Pagination { get; init; }           // LIMIT/OFFSET
}
```

**Example JSON**:
```json
{
  "filter": {
    "type": "and",
    "filters": [
      { "type": "leaf", "condition": { "fieldName": "Price", "operator": "GreaterThan", "value": 100 } },
      { "type": "leaf", "condition": { "fieldName": "IsActive", "operator": "Equals", "value": true } }
    ]
  },
  "projection": {
    "fields": [{ "fieldName": "Name" }, { "fieldName": "Category.Name" }]
  },
  "sort": {
    "sortFields": [{ "fieldName": "Name", "direction": "Ascending" }]
  },
  "pagination": { "pageNumber": 1, "pageSize": 20 }
}
```

#### FilterExpression (Abstract ADT)

**Variants**:
- `LeafFilter` - Single condition
- `AndFilter` - Logical AND (contains `IReadOnlyList<FilterExpression> Filters`)
- `OrFilter` - Logical OR (contains `IReadOnlyList<FilterExpression> Filters`)

**Composability**: Filters nest recursively for complex conditions

```csharp
// (Price > 100 AND IsActive = true) OR (Category.Name = "Electronics")
var filter = new OrFilter
{
    Filters = new[]
    {
        new AndFilter { Filters = new[] { priceFilter, activeFilter } },
        categoryFilter
    }
};
```

#### FilterCondition

**Operators Supported**:
- `Equals`, `NotEquals`
- `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`
- `Contains`, `StartsWith`, `EndsWith` (string operations)
- `In` (value in collection)
- `IsNull`, `IsNotNull`

```csharp
public sealed record FilterCondition
{
    public required string FieldName { get; init; }
    public required FilterOperator Operator { get; init; }
    public object? Value { get; init; }
}
```

#### ProjectionRequest

**Purpose**: Specifies which fields to include in results

```csharp
public sealed record ProjectionRequest
{
    public IReadOnlyList<FieldProjection> Fields { get; init; }
    public bool IsAllFields { get; init; }                    // true = SELECT *
    public bool IncludeRelatedDeep { get; init; }             // Include navigation properties
    public int Depth { get; init; }                           // Max nesting depth (1-7)
    public IReadOnlyDictionary<string, ProjectionField>? FieldTransformations { get; init; }
}
```

**Static Factory Methods**:
```csharp
ProjectionRequest.Default()           // All fields, shallow
ProjectionRequest.AllDirect()         // All direct fields only
ProjectionRequest.AllDeep(depth: 3)   // All fields with 3-level nesting
new ProjectionRequest(field1, field2) // Selective fields
```

**Depth Control** (1-7 levels):
- Prevents infinite recursion in self-referential entities (e.g., Category.ParentCategory)
- Balances performance vs completeness
- Default: 1 (one level of related entities)

#### ProjectionField (Abstract ADT for Transformations)

**Purpose**: Computed fields with transformations

**Variants**:
- `FieldReference` - Direct entity field
- `ConstantValue` - Literal value
- `StringTransform` - String operations (ToUpper, ToLower, Substring, Trim, etc.)
- `ConcatTransform` - Concatenate multiple fields
- `ArithmeticTransform` - Math operations (Add, Subtract, Multiply, Divide, etc.)
- `ConditionalTransform` - IF/THEN/ELSE logic
- `ComparisonTransform` - Boolean comparisons
- `Cast<max_thinking_length>19930</max_thinking_length>Transform` - Type conversions
- `CoalesceTransform` - NULL coalescing

**Example** (uppercase product name):
```json
{
  "projection": {
    "fieldTransformations": {
      "upperName": {
        "operation": "ToUpper",
        "input": { "fieldName": "Name" }
      }
    }
  }
}
```

#### SortRequest

```csharp
public sealed record SortRequest
{
    public IReadOnlyList<SortField> SortFields { get; init; }
}

public sealed record SortField
{
    public required string FieldName { get; init; }
    public SortDirection Direction { get; init; } = SortDirection.Ascending;
}
```

### Entity-to-Infrastructure Mapping

For each entity, the following infrastructure classes are created:

| Component | Naming Pattern | Location | Lines | Purpose |
|-----------|---------------|----------|-------|---------|
| Entity | `{Entity}` | BL/Entities/ | Variable | Core domain model |
| Query Builder | `{Entity}QueryBuilder` | BL/DataAccess/ | ~13 | ADT → LINQ expressions |
| Search Service | `{Entity}SearchService` | BL/Services/ | ~28 | Query orchestration |
| Query Controller | `{Entity}sQueryController` | API/Controllers/ | ~25 | HTTP endpoints |
| Projection Mapper | `{Entity}ProjectionMapper` | BL/Mappers/Projection/ | ~250 | Entity → DTO mapping |
| Projection Interface | `I{Entity}ProjectionMapper` | BL/Mappers/Projection/ | ~3 | Marker interface |
| Projection DTO | `{Entity}Projection` | DTO/ADTs/ | ~30 | DTO structure |
| Search Fields | `{Entity}SearchFields` | DTO/ADTs/ | ~40 | Field name constants |
| Validator | `{Entity}SearchQueryValidator` | BL/Validators/ | ~200 | Query validation |

**Total: ~589 lines per entity** (mostly concentrated in ProjectionMapper and Validator)

**Examples**:
- `Good` entity → `GoodQueryBuilder`, `GoodSearchService`, `GoodsQueryController`, `GoodProjectionMapper`, `IGoodProjectionMapper`, `GoodProjection`, `GoodSearchFields`, `GoodSearchQueryValidator`
- `Category` entity → `CategoryQueryBuilder`, `CategorySearchService`, `CategoriesQueryController`, etc.

### Projection Strategies

#### 1. All Fields (Shallow)

Returns all entity properties, one level of related entities

```csharp
var projection = ProjectionRequest.Default();
// Result: Good with all properties, Category with ID only
```

#### 2. All Fields (Deep)

Returns all entity properties, nested related entities up to specified depth

```csharp
var projection = ProjectionRequest.AllDeep(depth: 3);
// Result: Good → Category → ParentCategory → ParentCategory (3 levels)
```

**Depth Limits**: 1-7 levels enforced to prevent:
- Stack overflow in recursive structures
- Excessive SQL joins (performance)
- Circular reference issues

#### 3. Selective Fields

Returns only specified fields

```csharp
var projection = new ProjectionRequest(
    new FieldProjection { FieldName = "Name" },
    new FieldProjection { FieldName = "Price" },
    new FieldProjection { FieldName = "Category.Name" }
);
// Result: { "name": "...", "price": ..., "category": { "name": "..." } }
```

**Nested Field Syntax**: Use dot notation (`"Category.Name"`) for related entity fields

#### 4. Field Transformations

Returns computed fields using ProjectionField ADTs

```csharp
var projection = new ProjectionRequest
{
    FieldTransformations = new Dictionary<string, ProjectionField>
    {
        ["upperName"] = new StringTransform 
        { 
            Operation = StringOperation.ToUpper,
            Input = new FieldReference { FieldName = "Name" }
        },
        ["total"] = new ArithmeticTransform
        {
            Operation = ArithmeticOperation.Multiply,
            Left = new FieldReference { FieldName = "Price" },
            Right = new FieldReference { FieldName = "Quantity" }
        }
    }
};
// Result: TransformationResult with dynamic fields
```

**Execution**: Transformations execute **in-memory after database query** (client evaluation)

### Validator Pattern

Uses `DataModelMetadata` for runtime type-safe validation:

```csharp
public class GoodSearchQueryValidator : IValidator<SearchQuery>
{
    private static readonly IDataModelMetadata<Good> Metadata = DataModelMetadata.Good;
    
    public async Task<ValidationResult> ValidateAsync(SearchQuery query, CancellationToken ct = default)
    {
        var errors = new List<string>();
        
        if (query.Filter != null)
            ValidateFilter(query.Filter, errors);
        
        if (query.Projection != null)
            ValidateProjection(query.Projection, errors);
            
        return errors.Any() 
            ? ValidationResult.Invalid(errors)
            : ValidationResult.Valid();
    }
    
    private void ValidateFilterCondition(FilterCondition condition, List<string> errors)
    {
        // Check if field exists
        if (!Metadata.Properties.ContainsKey(condition.FieldName))
        {
            errors.Add($"Field '{condition.FieldName}' does not exist on entity Good");
            return;
        }
        
        var propertyMetadata = Metadata.Properties[condition.FieldName];
        
        // Validate type compatibility
        if (condition.Value != null)
        {
            var expectedType = propertyMetadata.Type;
            var actualType = condition.Value.GetType();
            
            if (!IsTypeCompatible(expectedType, actualType))
            {
                errors.Add($"Field '{condition.FieldName}' expects type {expectedType} but got {actualType}");
            }
        }
        
        // Validate operator compatibility
        if (condition.Operator == FilterOperator.Contains && propertyMetadata.Type != typeof(string))
        {
            errors.Add($"Contains operator only valid for string fields");
        }
    }
}
```

**Benefits**:
- No hardcoded field names in validator
- Type safety at runtime based on metadata
- Automatically stays in sync with entity changes
- Clear error messages for clients

### Dependency Injection Registration Pattern

Each entity requires careful DI registration following this pattern.

**Non-owned entity (standard):**
```csharp
// 1. Query Builder
builder.Services.AddScoped<IQueryBuilder<Good>, GoodQueryBuilder>();

// 2. Projection Mapper (dual registration: entity-specific + generic)
builder.Services.AddScoped<IGoodProjectionMapper, GoodProjectionMapper>();
builder.Services.AddScoped<IProjectionMapper<Good, GoodProjection>>(
    sp => sp.GetRequiredService<IGoodProjectionMapper>());

// 3. Validator
builder.Services.AddScoped<IValidator<SearchQuery>, GoodSearchQueryValidator>();

// 4. Search Service (dual registration: concrete + interface)
builder.Services.AddScoped<GoodSearchService>();
builder.Services.AddScoped<ISearchService<Good, GoodProjection>>(
    sp => sp.GetRequiredService<GoodSearchService>());

// 5. Query Controller (automatic via AddControllers)
```

**Shared Services** (register once per application):
```csharp
builder.Services.AddScoped<ProjectionExpressionBuilder>();
```

**Owned entity (ownership-aware):**
```csharp
// Register ownership services once per API project
builder.Services.AddUserTenantOwnershipServices(); // or AddOwnershipServices<TOwnership, TFactory>()

// 1. Owned Query Builder (requires ICurrentIdentityContext)
builder.Services.AddScoped<IQueryBuilder<Order, UserTenantOwnership>>(sp =>
    new OrderQueryBuilder(sp.GetRequiredService<ICurrentIdentityContext<UserTenantOwnership>>()));
builder.Services.AddScoped<IQueryBuilder<Order>>(
    sp => sp.GetRequiredService<IQueryBuilder<Order, UserTenantOwnership>>());

// 4. Owned Search Service
builder.Services.AddScoped<OrderSearchService>(sp => new OrderSearchService(
    sp.GetRequiredService<IRepository<Order>>(),
    sp.GetRequiredService<IQueryBuilder<Order, UserTenantOwnership>>(),
    sp.GetRequiredService<IProjectionMapper<Order, OrderProjection>>(),
    sp.GetRequiredService<ProjectionExpressionBuilder>(),
    sp.GetRequiredService<OrderSearchQueryValidator>(),
    sp.GetRequiredService<ICurrentIdentityContext<UserTenantOwnership>>(),
    sp.GetRequiredService<ILogger<OrderSearchService>>()));
builder.Services.AddScoped<ISearchService<Order, OrderProjection, UserTenantOwnership>>(
    sp => sp.GetRequiredService<OrderSearchService>());
builder.Services.AddScoped<ISearchService<Order, OrderProjection>>(
    sp => sp.GetRequiredService<OrderSearchService>());
```

> **Note**: In generated bounded contexts this DI wiring is emitted automatically by the code generator when `"owned": true` is set on the entity in the data model JSON.

**Why Dual Registration?**
- Entity-specific interfaces (e.g., `IGoodProjectionMapper`) allow type-safe nested mapper injection
- Generic interfaces (e.g., `IProjectionMapper<Good, GoodProjection>`) enable polymorphic service injection
- Both resolve to same singleton instance

### Adding New Entities to ADT-Based Search

**Steps to enable ADT-based querying for a new entity:**

1. **Create Entity** - Follow immutability pattern with private setters, dedicated constructor

2. **Create Infrastructure Classes** (8 files per entity):
   
   a. **QueryBuilder** (BL/DataAccess/{Entity}QueryBuilder.cs):
   ```csharp
   public class ProductQueryBuilder : BaseQueryBuilder<Product> { }
   ```
   
   b. **SearchService** (BL/Services/{Entity}SearchService.cs):
   ```csharp
   public class ProductSearchService : BaseSearchService<Product, ProductProjection>
   {
       public ProductSearchService(
           IRepository<Product> repository,
           IQueryBuilder<Product> queryBuilder,
           IProjectionMapper<Product, ProductProjection> projectionMapper,
           ProjectionExpressionBuilder expressionBuilder,
           IValidator<SearchQuery> validator,
           ILogger<ProductSearchService> logger)
           : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, logger) { }
   }
   ```
   
   c. **QueryController** (API/Controllers/{Entity}sQueryController.cs):
   ```csharp
   [ApiController]
   [Route("api/products/query")]
   [AllowAnonymous]
   public class ProductsQueryController : BaseQueryController<Product, ProductProjection>
   {
       public ProductsQueryController(
           ISearchService<Product, ProductProjection> searchService,
           ILogger<ProductsQueryController> logger)
           : base(searchService, logger) { }
   }
   ```
   
   d. **ProjectionMapper** (BL/Mappers/Projection/{Entity}ProjectionMapper.cs):
   - Implement 4 abstract methods
   - Handle nested entity projections
   - Use HashSet pattern for selective fields
   
   e. **ProjectionMapper Interface** (BL/Mappers/Projection/I{Entity}ProjectionMapper.cs):
   ```csharp
   public interface IProductProjectionMapper : IProjectionMapper<Product, ProductProjection> { }
   ```
   
   f. **Projection DTO** (DTO/ADTs/{Entity}Projection.cs):
   - All properties nullable
   - Match entity structure
   - Include related entity projections
   
   g. **SearchFields** (DTO/ADTs/{Entity}SearchFields.cs):
   - String constants for field names
   - Validation helpers
   
   h. **Validator** (BL/Validators/{Entity}SearchQueryValidator.cs):
   - Use `DataModelMetadata.{Entity}` for validation
   - Validate filters, projections, sorts

3. **Register in DI** - Follow dual registration pattern (see above)

4. **Test** - Entity immediately queryable via:
   - `POST /api/products/query` - Regular search
   - `POST /api/products/query/transform` - Field transformations

**Note**: Most of these classes are boilerplate and should be code-generated (see GENERATION.md)

### Best Practices

1. **Virtual Properties**: Override `ParameterName` and `EntityName` only when defaults don't fit
2. **Projection Depth**: Use `AllDeep(3)` as reasonable default, adjust based on performance
3. **Selective Projections**: For large entities, prefer selective fields to reduce payload size
4. **Transformations**: Use sparingly - they execute in-memory (client evaluation)
5. **Validation**: Always validate field names and types using validators
6. **Testing**: Test each layer independently using interface mocks
7. **Error Handling**: Let base classes handle exceptions, override only for custom logging
8. **Performance**: Monitor SQL queries, add indexes for frequently filtered/sorted fields

## Dependency Injection Rules
- All DataService implementations (e.g., CustomerService) must be injected as their corresponding IDataService<T...> or specific interface (e.g., ICustomerService), not as concrete types.
- All service, repository, and abstraction dependencies should be injected as interfaces, not concrete types, to maximize testability and flexibility.

## Testing Rules
- Every API project must have a corresponding unittest project (e.g., Inventorization.[BoundedContextName].API.Tests).
- Every concrete abstraction (service, repository, creator, modifier, query provider, etc.) must be covered with unit tests in the unittest project.

---

## Dependency Injection (DI) Project Pattern

**REQUIRED**: Every bounded context **must** have a dedicated DI project to centralize and standardize dependency injection configuration.

### DI Project Purpose

The DI project encapsulates all Service Collection registration logic for a bounded context, eliminating boilerplate from `Program.cs` and ensuring consistent registration patterns across all microservices.

### Project Structure

```
Inventorization.[BoundedContextName].DI/
├── Extensions/
│   └── [BoundedContextName]ServiceCollectionExtensions.cs
├── Inventorization.[BoundedContextName].DI.csproj
└── GlobalUsings.cs
```

### Project References

The DI project must reference:
- `Inventorization.Base` (for shared abstractions and base types)
- `Inventorization.[BoundedContextName].DTO` (for DTO types)
- `Inventorization.[BoundedContextName].BL` (for domain services, entities, and DbContext)
- `Inventorization.[BoundedContextName].Common` (for enums and constants, if applicable)

**Important**: DI project should NOT reference the API project to prevent circular dependencies.

### Service Collection Extension Pattern

All DI configuration for a bounded context is encapsulated in a static extension method:

**Example: CommerceServiceCollectionExtensions.cs**

```csharp
using Inventorization.Base.Abstractions;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Commerce.BL;
using Inventorization.Commerce.BL.DataServices;
using Inventorization.Commerce.BL.EntityConfigurations;
using Inventorization.Commerce.BL.Mappers;
using Inventorization.Commerce.BL.PropertyAccessors;
using Inventorization.Commerce.BL.SearchProviders;
using Inventorization.Commerce.BL.Validators;
using Inventorization.Commerce.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventorization.Commerce.DI.Extensions;

/// <summary>
/// Dependency Injection extension for Commerce bounded context.
/// Registers all domain services, repositories, mappers, validators, and data access components.
/// </summary>
public static class CommerceServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Commerce bounded context services to the dependency injection container.
    /// </summary>
    /// <example>
    /// usage in Program.cs:
    /// <code>
    /// builder.Services.AddCommerceServices(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddCommerceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext registration
        services.AddDbContext<CommerceDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("CommerceDb") ??
                throw new InvalidOperationException("CommerceDb connection string not found")));

        // Unit of Work
        services.AddScoped<ICommerceUnitOfWork, CommerceUnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Entity: Product
        services.AddScoped<IMapper<Product, ProductDetailsDTO>, ProductMapper>();
        services.AddScoped<IEntityCreator<Product, CreateProductDTO>, ProductCreator>();
        services.AddScoped<IEntityModifier<Product, UpdateProductDTO>, ProductModifier>();
        services.AddScoped<ISearchQueryProvider<Product, ProductSearchDTO>, ProductSearchProvider>();
        services.AddScoped<IValidator<CreateProductDTO>, CreateProductValidator>();
        services.AddScoped<IValidator<UpdateProductDTO>, UpdateProductValidator>();
        services.AddScoped<IProductDataService, ProductDataService>();

        // Entity: Order
        services.AddScoped<IMapper<Order, OrderDetailsDTO>, OrderMapper>();
        services.AddScoped<IEntityCreator<Order, CreateOrderDTO>, OrderCreator>();
        services.AddScoped<IEntityModifier<Order, UpdateOrderDTO>, OrderModifier>();
        services.AddScoped<ISearchQueryProvider<Order, OrderSearchDTO>, OrderSearchProvider>();
        services.AddScoped<IValidator<CreateOrderDTO>, CreateOrderValidator>();
        services.AddScoped<IValidator<UpdateOrderDTO>, UpdateOrderValidator>();
        services.AddScoped<IOrderDataService, OrderDataService>();

        // Additional relationship managers, services, etc.
        
        return services;
    }
}
```

When using in `Program.cs`:

```csharp
using Inventorization.Commerce.DI.Extensions;

var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services
    .AddCommerceServices(builder.Configuration)
    .AddGoodsServices(builder.Configuration)
    .AddAuthServices(builder.Configuration);

// ... rest of configuration
```

### Generation Pattern

The DI project structure and extension method are **automatically generated** by the code generator when scaffolding a new bounded context:

**Command:**
```bash
npm start generate examples/commerce-bounded-context.json -- --output-dir ../../backend
```

**Generated Files:**
- `Inventorization.Commerce.DI.csproj` (project file)
- `Extensions/CommerceServiceCollectionExtensions.cs` (DI extension)
- `GlobalUsings.cs` (common imports)

**Generation Details:**
- Extension method signature follows the pattern: `Add[ContextName]Services(this IServiceCollection, IConfiguration)`
- All DTO, domain service, mapper, validator, and creator/modifier registrations are generated based on metadata
- Entity relationships (many-to-many, one-to-many, one-to-one) generate corresponding relationship manager registrations
- DbContext connection string read from `[ContextName]Db` configuration key

### Regeneration Strategy

The DI extension is safe to regenerate:
- **Custom service registrations** can be added after the extension method call
- **Complex DI logic** (conditional registration, factory methods) added to the extension before the `return services;`
- **Custom dependencies** registered in separate extension methods and chained in `Program.cs`

Example with custom registration:

```csharp
builder.Services
    .AddCommerceServices(builder.Configuration)  // Generated extension
    .AddCustomCommerceServices();                 // Custom extension

public static IServiceCollection AddCustomCommerceServices(this IServiceCollection services)
{
    // Custom DI logic not generated
    services.Decorate<IProductDataService>();  // Decorator pattern
    services.AddCaching();                       // Custom caching logic
    return services;
}
```

### Validation

All DI registrations should be validated:

```csharp
public static IServiceCollection AddCommerceServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... registrations ...
    
    // Optional: Validate all registrations can be resolved (only in Development)
    var serviceProvider = services.BuildServiceProvider();
    serviceProvider.ValidateScopes();  // Throws if any scope validation fails
    
    return services;
}
```

### Benefits

- ✅ **Centralized DI configuration**: Single source of truth for all service registrations
- ✅ **Consistency across microservices**: Standard pattern in every bounded context
- ✅ **Reduced boilerplate**: Complex `Program.cs` reduced to one extension method call
- ✅ **Type-safe registration**: All registrations checked at compile-time via provided interfaces
- ✅ **Automatic generation**: Generated completely by code generator; custom logic added via separate extension methods
- ✅ **Testability**: Easy to mock/stub services in unit tests by registering test implementations
- ✅ **Maintainability**: Changes to entity structure automatically propagate to DI configuration during regeneration

### Required Elements in Every Service Collection Extension

1. **DbContext Registration** with connection string from configuration:
   ```csharp
   services.AddDbContext<[ContextName]DbContext>(options =>
       options.UseNpgsql(
           configuration.GetConnectionString("[ContextName]Db") ??
           throw new InvalidOperationException("[ContextName]Db connection string not found")));
   ```

2. **Unit of Work Registration** (both specific and base interfaces):
   ```csharp
   services.AddScoped<I[ContextName]UnitOfWork, [ContextName]UnitOfWork>();
   services.AddScoped<Inventorization.Base.DataAccess.IUnitOfWork>(sp =>
       sp.GetRequiredService<I[ContextName]UnitOfWork>());
   ```

3. **Generic Repository Registration**:
   ```csharp
   services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
   ```

4. **Entity-specific services** (mapper, creators, modifiers, validators, data service):
   ```csharp
   services.AddScoped<IMapper<[Entity], [Entity]DetailsDTO>, [Entity]Mapper>();
   services.AddScoped<IEntityCreator<[Entity], Create[Entity]DTO>, [Entity]Creator>();
   services.AddScoped<IEntityModifier<[Entity], Update[Entity]DTO>, [Entity]Modifier>();
   services.AddScoped<ISearchQueryProvider<[Entity], [Entity]SearchDTO>, [Entity]SearchProvider>();
   services.AddScoped<IValidator<Create[Entity]DTO>, Create[Entity]Validator>();
   services.AddScoped<IValidator<Update[Entity]DTO>, Update[Entity]Validator>();
   services.AddScoped<I[Entity]DataService, [Entity]DataService>();
   ```

5. **Relationship managers** (if applicable):
   ```csharp
   // Property accessors
   services.AddScoped<IEntityIdPropertyAccessor<[JunctionEntity]>, [JunctionEntity]EntityIdPropertyAccessor>();
   services.AddScoped<IRelatedEntityIdPropertyAccessor<[JunctionEntity]>, [JunctionEntity]RelatedEntityIdPropertyAccessor>();
   
   // Relationship metadata
   services.AddKeyedSingleton<IRelationshipMetadata<[Entity], [Related]>>(
       "[Relationship]",
       (sp, key) => DataModelRelationships.[RelationshipName]);
   
   // Relationship manager
   services.AddScoped<IRelationshipManager<[Entity], [Related]>>(sp =>
       new [Entity][Related]RelationshipManager(
           sp.GetRequiredService<IRepository<[Entity]>>(),
           sp.GetRequiredService<IRepository<[Related]>>(),
           sp.GetRequiredService<IRepository<[JunctionEntity]>>(),
           sp.GetRequiredService<IUnitOfWork>(),
           sp,
           sp.GetRequiredService<ILogger<[Entity][Related]RelationshipManager>>(),
           sp.GetRequiredKeyedService<IRelationshipMetadata<[Entity], [Related]>>("[Relationship]")));
   ```

---

## General Principles

### Project Naming Conventions

Each bounded context requires the following project structure:

  - **DTO Project**: `Inventorization.[BoundedContextName].DTO` (class library)
    - Purpose: Data Transfer Objects and mapping abstractions
    - Contents: DTOs organized in `DTO/[Entity]/` folders, mappers, validators
    
  - **Domain Project**: `Inventorization.[BoundedContextName].BL` (class library)
    - Purpose: Domain logic, entities, data access, business services
    - Contents: Entities, DbContexts, Unit of Work, business services, entity configurations, search providers
    
  - **API Project**: `Inventorization.[BoundedContextName].API` (ASP.NET web app)
    - Purpose: RESTful API endpoints and HTTP routing
    - Contents: Controllers, Swagger/OpenAPI configuration, middleware
    
  - **API Test Project**: `Inventorization.[BoundedContextName].API.Tests` (xUnit class library)
    - Purpose: Comprehensive unit test coverage for all backend abstractions
    - Contents: Data service tests, validator tests, mapper tests, search provider tests, instantiation tests
    - **Required for every API project** (generated automatically by code generator)
    
  - **DI Project**: `Inventorization.[BoundedContextName].DI` (class library)
    - Purpose: Centralized dependency injection configuration
    - Contents: ServiceCollectionExtensions for all bounded context registrations
    - **Required for every API project** (generated automatically by code generator)
    
  - **Common Project**: `Inventorization.[BoundedContextName].Common` (class library)
    - Purpose: Shared primitives and constants not belonging to Inventorization.Base
    - Contents: Domain-specific enums, value objects, constants
    
  - **Meta Project**: `Inventorization.[BoundedContextName].Meta` (class library)
    - Purpose: Centralized metadata repository for code generation and configuration
    - Contents: DataModelMetadata, DataModelRelationships static classes

### Meta Project Guidelines

Each bounded context **must** have a separate Meta project to store complete metadata about entities, properties, and relationships:

**Purpose**: Centralized metadata repository serving as single source of truth for:
- Entity structure and property definitions
- Validation rules and constraints
- EF Core configuration metadata
- UI form generation
- API documentation
- Code generation

**What belongs in Meta:**
- **DataModelMetadata**: Static class with `IDataModelMetadata<TEntity>` for each entity
- **DataModelRelationships**: Static class with `IRelationshipMetadata<TEntity, TRelated>` for all relationships
- Helper methods for metadata discovery

**Project References:**
- `Meta` project → references `Inventorization.Base` only (no other bounded context projects)
- `Domain` project → references `Meta` (for entity configurations, validators)
- `DTO` project → references `Meta` (optional, for validation metadata)
- `API` project → references `Meta` (transitive via Domain, for Swagger docs)

**Example Structure:**
```
Inventorization.Goods.Meta/
  ├── DataModelMetadata.cs (static class with entity metadata)
  ├── DataModelRelationships.cs (static class with relationship metadata)
  └── GlobalUsings.cs
```

**DataModelMetadata.cs Pattern:**
```csharp
public static class DataModelMetadata
{
    public static readonly IDataModelMetadata<Good> Good = new DataModelMetadataBuilder<Good>()
        .WithTable("Goods", schema: null)
        .WithDisplayName("Good")
        .WithDescription("Represents a product/item in the inventory system")
        .WithAuditing()
        .AddProperties(
            new DataPropertyMetadata(
                propertyName: nameof(Good.Name),
                propertyType: typeof(string),
                displayName: "Name",
                isRequired: true,
                maxLength: 200,
                description: "Name of the good"))
        .WithPrimaryKey(nameof(Good.Id))
        .AddIndex(nameof(Good.Sku))
        .Build();
        
    // Helper methods
    public static IReadOnlyList<IDataModelMetadata> GetAllEntityMetadata() => ...
}
```

**DataModelRelationships.cs Pattern:**
```csharp
public static class DataModelRelationships
{
    public static readonly IRelationshipMetadata<Good, Supplier> GoodSuppliers =
        new RelationshipMetadata<Good, Supplier>(
            type: RelationshipType.ManyToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(Good),
            relatedEntityName: nameof(Supplier),
            displayName: "Good Suppliers",
            junctionEntityName: nameof(GoodSupplier),
            navigationPropertyName: nameof(Good.GoodSuppliers),
            description: "Manages supplier relationships for goods");
}
```

**Benefits:**
- Single source of truth for all entity/relationship metadata
- Enables metadata-driven validation, configuration, and UI generation
- Separates metadata from domain logic
- Facilitates code generation and tooling
- Type-safe metadata access

### Common Project Guidelines
Each bounded context **should** have a separate Common project to store shared primitives that are used across multiple projects (DTO, Domain, API):

**Purpose**: Store commonly used primitives that don't belong in Inventorization.Base (which is for cross-bounded-context abstractions)

**What belongs in Common:**
- **Enums**: Domain-specific enumerations (e.g., `PurchaseOrderStatus`, `OrderType`)
- **Value Objects**: Immutable value types (e.g., `Money`, `Address`, `PhoneNumber`)
- **Constants**: Bounded context-specific constants (e.g., validation limits, business rules)
- **Custom Structs**: Domain-specific data structures

**Project References:**
- `DTO` project → references `Common`
- `Domain` project → references `Common`  
- `API` project → references `Common` (transitive via DTO/Domain)

**Anti-Pattern**: DO NOT reference `Domain.Entities` from DTO project just to use an enum. Extract the enum to `Common` instead.

**Example Structure:**
```
Inventorization.Goods.Common/
  ├── Enums/
  │   ├── PurchaseOrderStatus.cs
  │   └── StockMovementType.cs
  ├── Constants/
  │   └── ValidationLimits.cs
  └── GlobalUsings.cs
```

---

## Test Project Scaffolding & Infrastructure

**REQUIRED**: Every API project must have a corresponding test project (`Inventorization.[BoundedContextName].API.Tests`) with comprehensive unit test coverage for all domain abstractions.

### Test Project Purpose

The test project provides automated unit tests for:
- **Data Services**: CRUD operations, pagination, filtering, error handling
- **Validators**: Input validation, edge cases, business rule validation
- **Mappers**: DTO-to-entity and entity-to-DTO mapping, LINQ projections
- **Search Providers**: Query building, filtering expressions, pagination
- **Entity Creators & Modifiers**: Entity instantiation and state mutations
- **Instantiation**: Entity construction, validation, and immutability

### Project Structure

```
Inventorization.[BoundedContextName].API.Tests/
├── Inventorization.[BoundedContextName].API.Tests.csproj
├── GlobalUsings.cs
├── Services/                          # Data service tests
│   ├── [Entity]DataServiceTests.cs
│   └── ...
├── Validators/                        # Validator tests
│   ├── Create[Entity]ValidatorTests.cs
│   ├── Update[Entity]ValidatorTests.cs
│   └── ...
├── Mappers/                           # Mapper tests
│   ├── [Entity]MapperTests.cs
│   └── ...
├── SearchProviders/                   # Search provider tests
│   ├── [Entity]SearchProviderTests.cs
│   └── ...
├── Creators/                          # Entity creator tests
│   ├── [Entity]CreatorTests.cs
│   └── ...
├── Modifiers/                         # Entity modifier tests
│   ├── [Entity]ModifierTests.cs
│   └── ...
└── Instantiation/                     # Entity instantiation tests
    ├── [Entity]InstantiationTests.cs
    └── ...
```

### Test Framework Stack

All test projects use:

- **xUnit**: Test framework with [Fact] and [Theory] attributes
- **FluentAssertions**: Fluent assertion syntax for readable test assertions
- **Moq**: Mocking library for interface-based dependencies
- **Entity Framework Core InMemory**: Real DbContext for realistic async query testing (no query provider mocking)

**Global Usings** (`GlobalUsings.cs`):

```csharp
global using Xunit;
global using FluentAssertions;
global using Moq;
global using Inventorization.Base.Abstractions;
global using Inventorization.Base.Services;
global using Inventorization.[BoundedContextName].BL;
global using Inventorization.[BoundedContextName].BL.DataServices;
global using Inventorization.[BoundedContextName].BL.Validators;
global using Inventorization.[BoundedContextName].BL.Mappers;
global using Inventorization.[BoundedContextName].BL.SearchProviders;
global using Inventorization.[BoundedContextName].BL.Creators;
global using Inventorization.[BoundedContextName].BL.Modifiers;
global using Inventorization.[BoundedContextName].DTO;
global using Inventorization.[BoundedContextName].Common.Enums;
global using Microsoft.EntityFrameworkCore;
```

### Generation Pattern

Test projects are **automatically generated** by the code generator when scaffolding a new bounded context:

**Command:**
```bash
npm start generate examples/commerce-bounded-context.json -- --output-dir ../../backend
```

**Generated Test Files** (per entity):
1. **Data Service Tests**: `Services/[Entity]DataServiceTests.cs`
   - Tests CRUD operations (GetById, Create, Update, Delete)
   - Tests pagination and filtering
   - Tests error handling (not found, validation errors)
   
2. **Validator Tests**: `Validators/Create[Entity]ValidatorTests.cs`, `Update[Entity]ValidatorTests.cs`
   - Tests valid inputs pass validation
   - Tests invalid inputs fail with appropriate error messages
   - Tests edge cases (empty strings, null values, boundary values)
   
3. **Mapper Tests**: `Mappers/[Entity]MapperTests.cs`
   - Tests DTO-to-entity mapping
   - Tests entity-to-DTO projection
   - Tests null handling and type conversions
   
4. **Search Provider Tests**: `SearchProviders/[Entity]SearchProviderTests.cs`
   - Tests query building with EF Core InMemory DbContext
   - Tests filtering expressions
   - Tests pagination logic
   - Uses real `DbContextOptionsBuilder<[Context]DbContext>().UseInMemoryDatabase()` for each test
   
5. **Creator Tests**: `Creators/[Entity]CreatorTests.cs`
   - Tests entity instantiation from CreateDTO
   - Tests validation of input data
   - Tests entity properties are set correctly
   
6. **Modifier Tests**: `Modifiers/[Entity]ModifierTests.cs`
   - Tests entity state updates from UpdateDTO
   - Tests modified properties are updated
   - Tests immutable properties remain unchanged
   
7. **Instantiation Tests**: `Instantiation/[Entity]InstantiationTests.cs`
   - Tests entity can be instantiated with valid data
   - Tests entity properties have expected values after construction
   - Tests entity immutability constraints
   - Part of validation-driven development (ensuring entities behave as designed)

### Search Provider Test Pattern (EF Core InMemory)

Search provider tests use **Entity Framework Core InMemory** database for realistic async query testing:

```csharp
[Fact]
public async Task SearchAsync_WithValidFilter_ReturnsMatchingEntities()
{
    // Arrange
    var options = new DbContextOptionsBuilder<CommerceDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    await using var context = new CommerceDbContext(options);
    
    var product1 = new Product(name: "SKU-1", status: ProductStatus.Active);
    var product2 = new Product(name: "SKU-2", status: ProductStatus.Inactive);
    
    context.Products.Add(product1);
    context.Products.Add(product2);
    await context.SaveChangesAsync();
    
    var searchProvider = new ProductSearchProvider();
    var searchDTO = new ProductSearchDTO
    {
        Filter = new ProductFilterDTO { Status = ProductStatus.Active },
        Page = new PageDTO { PageNumber = 1, PageSize = 10 }
    };
    
    // Act
    var query = context.Products.Where(searchProvider.BuildQuery(searchDTO));
    var results = await query.ToListAsync();
    
    // Assert
    results.Should().HaveCount(1);
    results.First().Name.Should().Be("SKU-1");
}
```

**Why InMemory DbContext instead of mocking IQueryable?**
- Properly supports async operations (`ToListAsync()`, `CountAsync()`, etc.)
- Executes LINQ-to-Objects, catching query bugs at test time
- No need for custom `TestAsyncQueryProvider` or mocking complexities
- Simulates real database behavior for accurate testing
- Tests run fast (in-memory, no network I/O)

### Entity Instantiation Test Pattern

Instantiation tests verify entity construction and immutability:

```csharp
[Fact]
public void Product_Constructor_InitializesPropertiesCorrectly()
{
    // Arrange
    var name = "Test Product";
    var sku = "SKU-001";
    var status = ProductStatus.Active;
    
    // Act
    var product = new Product(name: name, sku: sku, status: status);
    
    // Assert
    product.Name.Should().Be(name);
    product.Sku.Should().Be(sku);
    product.Status.Should().Be(status);
    product.IsActive.Should().BeTrue();
    product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
}

[Fact]
public void Product_UpdatePrice_ModifiesPropertyAndUpdatesTimestamp()
{
    // Arrange
    var product = new Product(name: "Original", sku: "SKU-001", status: ProductStatus.Active);
    var originalCreatedAt = product.CreatedAt;
    var newPrice = 99.99m;
    
    // Act
    System.Threading.Thread.Sleep(100);  // Ensure time difference
    product.UpdatePrice(newPrice);
    
    // Assert
    product.Price.Should().Be(newPrice);
    product.UpdatedAt.Should().NotBeNull();
    product.UpdatedAt.Should().BeAfter(originalCreatedAt);
    product.CreatedAt.Should().Be(originalCreatedAt);  // CreatedAt never changes
}

[Fact]
public void Product_Properties_CannotBeDirectlySet()
{
    // Arrange
    var product = new Product(name: "Test", sku: "SKU-001", status: ProductStatus.Active);
    
    // Act & Assert
    var property = typeof(Product).GetProperty(nameof(Product.Name));
    property.SetMethod.Should().BeNull();  // Verify no public setter
}
```

### Sample Data Generation

The test generator produces valid sample data respecting:

- **Regex Patterns**: For properties with regex constraints
  - Pattern `^[A-Z0-9-]+$` generates `"SKU-1"`
  - Pattern `^ORD-[0-9]{8}$` generates `"ORD-00000001"`
  - Pattern `^[A-Z]{2}[0-9]{3}$` generates `"AB123"`

- **Email Fields**: Auto-detected email properties
  - Sample value: `"user@example.com"`

- **String Properties**: Random strings of appropriate length
  - Respects `MaxLength` constraints
  - Example: 100-character `Description` generates a realistic string

- **Numeric Properties**: Valid ranges for type
  - `decimal` generates realistic prices (e.g., `99.99)
  - `int` generates valid IDs or counts
  - `DateTime` generates appropriate timestamps

### Test Coverage Requirements

Every bounded context **must** achieve minimum test coverage:

| Component | Coverage | Notes |
|-----------|----------|-------|
| Data Services | 90%+ | CRUD ops, error cases, pagination |
| Validators | 100% | All validation rules tested |
| Mappers | 95%+ | Forward/reverse mapping, null handling |
| Search Providers | 85%+ | Query building, filtering |
| Entity Creators | 100% | All constructor paths tested |
| Entity Modifiers | 100% | All state mutation methods tested |
| Instantiation | 100% | All entities have instantiation tests |

### Run Tests

Generate tests for a bounded context:

```bash
# Generate test project for Commerce bounded context
npm start generate examples/commerce-bounded-context.json -- --output-dir ../../backend

# Navigate to test project
cd backend/Inventorization.Commerce.API.Tests/

# Run all tests
dotnet test

# Run specific test fixture
dotnet test --filter "ClassName=CommerceNamespace.ProductDataServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Regeneration Strategy

Tests are safe to regenerate:
- **Test templates** are completely regenerated - any manual test edits will be overwritten
- **Test fixtures** should NOT be manually edited (regenerate instead)
- **Custom test logic** should go in separate test files NOT generated (e.g., `ProductDataServiceIntegrationTests.cs`)
- **Configuration and setup** can be customized in test classes after generation (just before/after class definitions)

### Benefits

- ✅ **Comprehensive coverage**: Generated tests cover all data service methods, validators, mappers, search providers
- ✅ **Consistent patterns**: All tests follow identical structure and assertion styles
- ✅ **Fast execution**: InMemory databases, no network I/O, tests complete in milliseconds
- ✅ **Type safety**: Compile-time validation of all test assertions
- ✅ **Realistic data**: Sample data generation respects regex patterns, email formats, constraints
- ✅ **Maintainability**: Changes to entity structure automatically propagate to tests during regeneration
- ✅ **CI/CD ready**: Tests run in headless environment without dependencies
- ✅ **Debugging support**: Detailed assertions using FluentAssertions provide clear failure messages

## Base Abstractions and Data Structures
- All base abstractions and common data structures (such as `CreateDTO`, `UpdateDTO`, `DeleteDTO`, `DetailsDTO`, `SearchDTO`, `PageDTO`, `ServiceResult<T>`, `UnitOfWorkBase<TDbContext>`, and all generic interfaces like `IEntityCreator`, `IEntityModifier`, `ISearchQueryProvider`, `IMapper`, `IPropertyAccessor`, `IValidator`, `IUnitOfWork`, etc.) must be located in a separate shared project named `Inventorization.Base`.
- All bounded context/domain projects must reference `Inventorization.Base` for these shared types.
- All DTOs for each bounded context must be placed in the corresponding DTO project under a `DTO/` subfolder (e.g., `DTO/Customer`).
- All mapping logic (entity-to-DTO and DTO-to-entity) must use the `IMapper<TEntity, TDetailsDTO>` abstraction, supporting both object mapping and LINQ projection via `Expression<Func<TEntity, TDetailsDTO>>`.
- All property access logic must use the `IPropertyAccessor<TEntity, TProperty>` abstraction, supporting both expression access and compiled getter caching for performance.

## Unit of Work Pattern

All bounded contexts must use the **generic `UnitOfWorkBase<TDbContext>`** class located in `Inventorization.Base.DataAccess` for implementing the Unit of Work pattern. This eliminates boilerplate transaction management code and ensures consistent behavior across all bounded contexts.

### UnitOfWork Implementation in Bounded Context

Each bounded context requires minimal concrete code for its Unit of Work:

**Step 1: Define bounded context interface** (inherits from `IUnitOfWork`):

```csharp
// In BoundedContext.BL/DataAccess/
public interface IGoodsUnitOfWork : Inventorization.Base.DataAccess.IUnitOfWork
{
    // Add bounded context-specific methods if needed (usually empty)
}
```

**Step 2: Implement concrete UnitOfWork** (inherits from `UnitOfWorkBase<TDbContext>`):

```csharp
// In BoundedContext.BL/DataAccess/
using Inventorization.Base.DataAccess;
using Microsoft.Extensions.Logging;

public class GoodsUnitOfWork : UnitOfWorkBase<GoodsDbContext>, IGoodsUnitOfWork
{
    public GoodsUnitOfWork(GoodsDbContext context, ILogger<GoodsUnitOfWork> logger)
        : base(context, logger)
    {
    }
    
    // Override virtual methods only if custom behavior needed
}
```

**That's it!** All transaction management, change tracking, and disposal logic is inherited from `UnitOfWorkBase`.

### UnitOfWorkBase Features

The `UnitOfWorkBase<TDbContext>` class provides:

- ✅ **Complete IUnitOfWork implementation** with all 6 methods
- ✅ **Transaction management**: Begin, Commit, Rollback with state validation
- ✅ **Comprehensive logging**: Transaction start/commit/rollback, change counts, disposal warnings
- ✅ **Error handling**: Try-catch blocks with detailed error logging
- ✅ **Safe disposal**: Automatic rollback of active transactions on dispose
- ✅ **Virtual methods**: All methods can be overridden for specialized behavior
- ✅ **Type safety**: Generic constraint ensures only `DbContext` types accepted

### UnitOfWork Methods

All methods inherited from `UnitOfWorkBase`:

```csharp
// Save all tracked changes
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)

// Begin a new transaction
Task BeginTransactionAsync(CancellationToken cancellationToken = default)

// Commit current transaction
Task CommitTransactionAsync(CancellationToken cancellationToken = default)

// Rollback current transaction
Task RollbackTransactionAsync(CancellationToken cancellationToken = default)

// Async disposal with transaction cleanup
ValueTask DisposeAsync()

// Sync disposal with transaction cleanup
void Dispose()
```

### Dependency Injection Registration

Register both the specific and base interfaces in `Program.cs`:

```csharp
// UnitOfWork
builder.Services.AddScoped<IGoodsUnitOfWork, GoodsUnitOfWork>();
builder.Services.AddScoped<Inventorization.Base.DataAccess.IUnitOfWork>(sp => 
    sp.GetRequiredService<IGoodsUnitOfWork>());
```

**Note**: The `ILogger<T>` is automatically injected by the DI container—no special configuration needed.

### Design Principles

1. **DRY (Don't Repeat Yourself)**: Eliminates ~70-100 lines of duplicate code per bounded context
2. **Consistency**: All bounded contexts have identical transaction behavior and logging
3. **Testability**: Base class can be unit tested once; concrete implementations inherit reliability
4. **Extensibility**: Override `virtual` methods for custom behavior when needed
5. **Minimal Code**: Each bounded context UnitOfWork is typically 7-10 lines (interface + constructor)

### All UnitOfWork Implementations Must Use This Pattern

All UnitOfWork implementations must:
- Inherit from `UnitOfWorkBase<TDbContext>` where `TDbContext` is the bounded context's `DbContext`
- Implement the bounded context-specific interface (e.g., `IGoodsUnitOfWork`)
- Inject both `TDbContext` and `ILogger<T>` in the constructor
- Have minimal concrete code unless specialized behavior is required
- Be registered in DI as both specific and base `IUnitOfWork` interfaces

---

## Entity Configuration Pattern

All bounded contexts **must** use the `IEntityTypeConfiguration<T>` pattern with base configuration classes to eliminate DbContext boilerplate. This pattern reduces DbContext size by 70-95% and centralizes entity configuration logic.

### The Problem

Without this pattern, DbContext files become massive (300+ lines) with repetitive inline configuration:

```csharp
// ❌ ANTI-PATTERN: Inline configuration (don't do this)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.ToTable("Users");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
        entity.HasIndex(e => e.Email).IsUnique();
        // ... 20+ more lines per entity
    });
    
    modelBuilder.Entity<Role>(entity =>
    {
        // ... another 20+ lines
    });
    
    // ... repeated for every entity
}
```

### The Solution

Use separated `IEntityTypeConfiguration<T>` implementations with base classes:

**1. Base Configuration Classes** (in `Inventorization.Base.DataAccess`)

```csharp
/// <summary>
/// Base configuration for all entities extending BaseEntity.
/// Handles table naming and primary key configuration.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Table name (simple pluralization)
        builder.ToTable(GetTableName());
        
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Call derived class configuration
        // Note: Derived classes should explicitly configure their own indexes
        // for properties like CreatedAt, IsActive, etc. for type safety
        ConfigureEntity(builder);
    }
    
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    
    protected virtual string GetTableName() => typeof(TEntity).Name + "s";
}

/// <summary>
/// Base configuration for junction entities in many-to-many relationships.
/// Handles composite key, unique index, and relationship metadata.
/// </summary>
public abstract class JunctionEntityConfiguration<TJunction, TEntity, TRelated> 
    : BaseEntityConfiguration<TJunction>
    where TJunction : JunctionEntityBase
    where TEntity : BaseEntity
    where TRelated : BaseEntity
{
    protected readonly IRelationshipMetadata<TEntity, TRelated> Metadata;
    
    protected JunctionEntityConfiguration(IRelationshipMetadata<TEntity, TRelated> metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        
        if (Metadata.Type != RelationshipType.ManyToMany)
            throw new InvalidOperationException(
                $"JunctionEntityConfiguration requires ManyToMany relationship type. " +
                $"Provided: {Metadata.Type}");
    }
    
    protected override void ConfigureEntity(EntityTypeBuilder<TJunction> builder)
    {
        // Composite unique index on EntityId + RelatedEntityId
        builder.HasIndex(e => new { e.EntityId, e.RelatedEntityId }).IsUnique();
        
        // Call derived class for relationship and metadata configuration
        ConfigureJunctionEntity(builder);
    }
    
    protected abstract void ConfigureJunctionEntity(EntityTypeBuilder<TJunction> builder);
}
```

**2. DataModelRelationships Static Class** (in each BoundedContext.Domain)

Single source of truth for all relationship metadata, used by configurations, relationship managers, and DI:

```csharp
/// <summary>
/// Centralized repository of all relationship metadata for the Auth bounded context.
/// Single source of truth for entity relationships.
/// </summary>
public static class DataModelRelationships
{
    /// <summary>
    /// User ↔ Role many-to-many relationship via UserRole junction
    /// </summary>
    public static readonly IRelationshipMetadata<User, Role> UserRoles =
        new RelationshipMetadata<User, Role>(
            type: RelationshipType.ManyToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(User),
            relatedEntityName: nameof(Role),
            displayName: "User Roles",
            junctionEntityName: nameof(UserRole),
            navigationPropertyName: nameof(User.UserRoles),
            description: "Manages the many-to-many relationship between users and their assigned roles");

    /// <summary>
    /// User → RefreshToken one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<User, RefreshToken> UserRefreshTokens =
        new RelationshipMetadata<User, RefreshToken>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(User),
            relatedEntityName: nameof(RefreshToken),
            displayName: "User Refresh Tokens",
            navigationPropertyName: nameof(User.RefreshTokens),
            description: "Manages the one-to-many relationship between users and their refresh tokens");
}
```

**3. Concrete Entity Configurations** (in BoundedContext.BL/EntityConfigurations/)

Each entity gets its own configuration file inheriting from appropriate base:

```csharp
// Regular entity configuration
public class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.IsActive)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        // Explicitly add indexes for common query patterns (type-safe)
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedAt);
    }
}

// Junction entity configuration
public class UserRoleConfiguration : JunctionEntityConfiguration<UserRole, User, Role>
{
    public UserRoleConfiguration() : base(DataModelRelationships.UserRoles)
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<UserRole> builder)
    {
        // Configure relationships - CASCADE DELETE PROHIBITED
        builder.HasOne(e => e.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

**4. Simplified DbContext** (in BoundedContext.BL/DbContexts/)

```csharp
public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
```

**That's it!** DbContext reduced from 120+ lines to ~27 lines (77% reduction).

### Folder Structure

Each bounded context must have:

```
BoundedContext.BL/
  ├── DbContexts/
  │   └── BoundedContextDbContext.cs (simplified)
  ├── EntityConfigurations/
  │   ├── UserConfiguration.cs
  │   ├── RoleConfiguration.cs
  │   ├── UserRoleConfiguration.cs (junction)
  │   └── ... (one file per entity)
  ├── Entities/
  │   ├── User.cs
  │   ├── Role.cs
  │   └── ...
  └── DataModelRelationships.cs (static class)
```

### Benefits

- ✅ **70-95% DbContext size reduction**: 300+ lines → 20-30 lines
- ✅ **Separation of concerns**: Each entity configuration in its own file
- ✅ **Reduced boilerplate**: Base classes handle table naming, primary keys, common indexes
- ✅ **Type safety**: Compile-time validation of configurations
- ✅ **Testability**: Unit test each configuration independently
- ✅ **Consistency**: All entities follow identical patterns
- ✅ **Maintainability**: Changes to entity configuration isolated to single file
- ✅ **Automatic discovery**: `ApplyConfigurationsFromAssembly()` finds all configurations
- ✅ **Centralized metadata**: `DataModelRelationships` is single source of truth

### Rules for All Bounded Contexts

All bounded context DbContext implementations **must**:

1. **Use `ApplyConfigurationsFromAssembly()`** instead of inline configuration
2. **Create EntityConfigurations folder** with one file per entity
3. **Inherit from `BaseEntityConfiguration<TEntity>`** for regular entities
4. **Inherit from `JunctionEntityConfiguration<TJunction, TEntity, TRelated>`** for junction entities
5. **Create `DataModelRelationships` static class** with all relationship metadata
6. **Pass metadata from `DataModelRelationships`** to junction configurations
7. **Set `DeleteBehavior.Restrict`** on all foreign key relationships (CASCADE DELETE PROHIBITED)
8. **Keep DbContext under 50 lines** (DbSets + `ApplyConfigurationsFromAssembly()`)

### Example Impact

**Before (inline configuration):**
```csharp
// GoodsDbContext.cs - 340 lines
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 340 lines of inline entity configuration
}
```

**After (configuration classes):**
```csharp
// GoodsDbContext.cs - 27 lines (93% reduction)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodsDbContext).Assembly);
}

// + 9 separate configuration files in EntityConfigurations/ folder
// + 1 DataModelRelationships.cs static class
```

### Configuration Class Template

**Regular Entity:**
```csharp
using Inventorization.Base.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.[BoundedContext].Domain.EntityConfigurations;

public class [Entity]Configuration : BaseEntityConfiguration<[Entity]>
{
    protected override void ConfigureEntity(EntityTypeBuilder<[Entity]> builder)
    {
        // Property configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(e => e.Name)
            .IsUnique();
        
        // Explicitly configure indexes for query patterns (type-safe)
        // Only add indexes if you have queries that filter/sort by these fields
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.CreatedAt);
        
        // Relationships
        builder.HasOne(e => e.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

**Junction Entity:**
```csharp
using Inventorization.Base.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.[BoundedContext].Domain.EntityConfigurations;

public class [Junction]Configuration : JunctionEntityConfiguration<[Junction], [Entity], [Related]>
{
    public [Junction]Configuration() : base(DataModelRelationships.[RelationshipName])
    {
    }
    
    protected override void ConfigureJunctionEntity(EntityTypeBuilder<[Junction]> builder)
    {
        // Optional: metadata columns specific to this junction
        builder.Property(e => e.AssignedAt)
            .IsRequired();
        
        // Relationships - CASCADE DELETE PROHIBITED
        builder.HasOne(e => e.[Entity])
            .WithMany(x => x.[Junctions])
            .HasForeignKey(e => e.[Entity]Id)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.[Related])
            .WithMany(x => x.[Junctions])
            .HasForeignKey(e => e.[Related]Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## Smart Enum Pattern

The system uses **Smart Enums** (type-safe enumeration classes) instead of traditional C# enums to provide:
- **String serialization** in JSON API responses (e.g., `"Active"` instead of `1`)
- **Integer storage** in the database (efficient storage and indexing)
- **Type safety** with compile-time checking
- **Extensibility** with methods and properties

### Implementation

All Smart Enums inherit from `Enumeration` base class in `Inventorization.Base.Models`:

```csharp
[JsonConverter(typeof(EnumerationJsonConverter<ProductStatus>))]
public sealed class ProductStatus : Enumeration
{
    public static readonly ProductStatus Draft = new(nameof(Draft), 0);
    public static readonly ProductStatus Active = new(nameof(Active), 1);
    public static readonly ProductStatus OutOfStock = new(nameof(OutOfStock), 2);
    public static readonly ProductStatus Discontinued = new(nameof(Discontinued), 3);

    private ProductStatus(string name, int value) : base(name, value) { }

    public static ProductStatus FromName(string name) => FromNameOrThrow<ProductStatus>(name);
    public static ProductStatus FromValue(int value) => FromValueOrThrow<ProductStatus>(value);
    public static IEnumerable<ProductStatus> GetAll() => Enumeration.GetAll<ProductStatus>();
}
```

### Entity Configuration

Smart Enums require EF Core value converters in entity configurations:

```csharp
public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion(new EnumerationConverter<ProductStatus>());
    }
}
```

### JSON Serialization

JSON converter handles string serialization:
- **Request (accepts both)**: `{"status": "Active"}` or `{"status": 1}`
- **Response (always string)**: `{"status": "Active"}`

### Limitations

**Cannot use in switch statements** - Smart Enums are reference types, not compile-time constants:

```csharp
// ❌ INCORRECT - Won't compile
switch (product.Status)
{
    case ProductStatus.Draft:
        // ...
        break;
}

// ✅ CORRECT - Use if-else
if (product.Status == ProductStatus.Draft)
{
    // ...
}
else if (product.Status == ProductStatus.Active)
{
    // ...
}
```

### Location

- **Base Class**: `Inventorization.Base/Models/Enumeration.cs`
- **EF Converter**: `Inventorization.Base/DataAccess/EnumerationConverter.cs`
- **JSON Converter**: `Inventorization.Base/Models/EnumerationJsonConverter.cs`
- **Bounded Context Enums**: `Inventorization.[BoundedContext].Common/Enums/`

---

## Domain Service Abstractions

All bounded contexts should use the **`DataServiceBase`** class located in `Inventorization.Base.Services` for implementing data services. Two variants exist:

| Variant | Type parameters | When to use |
|---|---|---|
| Non-owned | `DataServiceBase<TEntity, TCreate, TUpdate, TDelete, TDetails, TSearch>` | Entity has no ownership (e.g. lookup/reference data) |
| Owned | `DataServiceBase<TOwnership, TEntity, TCreate, TUpdate, TDelete, TDetails, TSearch>` | Entity is owned by a user/tenant |

The owned variant constructor-injects `ICurrentIdentityContext<TOwnership>` and automatically stamps `Ownership` on create/update. This eliminates boilerplate CRUD code while enforcing consistent patterns across all contexts.

### Creating Data Services in a Bounded Context

Each entity requires:

#### 1. DTOs (in BoundedContext.DTO project)
- `DetailsDTO`: returned by `GetByIdAsync`
- `CreateDTO`: input for `AddAsync`
- `UpdateDTO`: input for `UpdateAsync`
- `DeleteDTO`: input for `DeleteAsync`
- `SearchDTO`: input for `SearchAsync`

All base DTOs should be defined in `Inventorization.Base` and inherited/extended in each bounded context as needed.

#### 2. Concrete Data Service (in BoundedContext.Domain project)

**Step-by-step examples:**

```csharp
// Non-owned entity (lookup / reference data)
public interface ICustomerDataService : IDataService<
    Customer, CreateCustomerDTO, UpdateCustomerDTO,
    DeleteCustomerDTO, CustomerDetailsDTO, CustomerSearchDTO> { }

public class CustomerDataService
    : DataServiceBase<Customer, CreateCustomerDTO, UpdateCustomerDTO,
                      DeleteCustomerDTO, CustomerDetailsDTO, CustomerSearchDTO>,
      ICustomerDataService
{
    public CustomerDataService(
        IUnitOfWork unitOfWork,
        IRepository<Customer> repository,
        ILogger<CustomerDataService> logger)
        : base(unitOfWork, repository, logger) { }
}

// Owned entity — automatic ownership stamping on Add/Update
public interface IOrderDataService : IDataService<
    Order, CreateOrderDTO, UpdateOrderDTO,
    DeleteOrderDTO, OrderDetailsDTO, OrderSearchDTO> { }

public class OrderDataService
    : DataServiceBase<UserTenantOwnership,
                      Order, CreateOrderDTO, UpdateOrderDTO,
                      DeleteOrderDTO, OrderDetailsDTO, OrderSearchDTO>,
      IOrderDataService
{
    public OrderDataService(
        IUnitOfWork unitOfWork,
        IRepository<Order> repository,
        ICurrentIdentityContext<UserTenantOwnership> identityContext,
        ILogger<OrderDataService> logger)
        : base(unitOfWork, repository, identityContext, logger) { }
}
```

**That's it!** No need to implement GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, or SearchAsync—all logic is inherited from `DataServiceBase`.

### DataServiceBase Features

The `DataServiceBase` class provides:

- ✅ **All 5 CRUD/Search methods** with complete error handling and logging
- ✅ **Lazy dependency resolution** via `IServiceProvider` for minimal memory footprint
- ✅ **Direct repository injection** (`IRepository<TEntity>`) for efficient data access
- ✅ **Automatic validation** using injected validators at runtime
- ✅ **Dynamic entity naming** via reflection for consistent log messages
- ✅ **Pagination support** via reflection on `SearchDTO.Page` property

### Dependency Resolution Pattern

`DataServiceBase` uses explicit constructor injection:

**Non-owned constructor:**
```csharp
IUnitOfWork unitOfWork,              // For SaveChangesAsync
IRepository<TEntity> repository,     // For CRUD operations
ILogger<...> logger                  // For logging
```

**Owned constructor (adds identity context):**
```csharp
IUnitOfWork unitOfWork,
IRepository<TEntity> repository,
ICurrentIdentityContext<TOwnership> identityContext,  // ← caller's ownership
ILogger<...> logger
```

**Runtime resolution (resolved via IServiceProvider only when needed):**
- `IMapper<TEntity, TDetailsDTO>` — GetByIdAsync, AddAsync, UpdateAsync, SearchAsync
- `IValidator<TCreateDTO>` — AddAsync
- `IValidator<TUpdateDTO>` — UpdateAsync
- `IEntityCreator<TEntity, TCreateDTO>` — AddAsync
- `IEntityModifier<TEntity, TUpdateDTO>` — UpdateAsync
- `ISearchQueryProvider<TEntity, TSearchDTO>` — SearchAsync

### Required Component Implementations

For each bounded context, you must implement:

#### Entity Mapping Abstractions
- `IMapper<TEntity, TDetailsDTO>`: provides both object mapping and LINQ projection for entity-to-DTO mapping
- `IEntityCreator<TEntity, TCreateDTO>`: creates entity from `CreateDTO`
- `IEntityModifier<TEntity, TUpdateDTO>`: updates entity from `UpdateDTO`
- `ISearchQueryProvider<TEntity, TSearchDTO>`: creates LINQ expression for search

#### Validation
- `IValidator<TCreateDTO>`: validates CreateDTO before entity creation
- `IValidator<TUpdateDTO>`: validates UpdateDTO before entity modification

### DTO Typing Rules
- Each entity has a corresponding set of DTOs, all located in the DTO project:
  - `DetailsDTO`: returned by `GetByIdAsync`
  - `CreateDTO`: input for `AddAsync`
  - `UpdateDTO`: input for `UpdateAsync` (must inherit from `UpdateDTO` base)
  - `DeleteDTO`: input for `DeleteAsync` (must inherit from `DeleteDTO` base)
  - `SearchDTO`: input for `SearchAsync`
- Each concrete `DataService` is generic over all relevant DTOs and inherits from `DataServiceBase`.

---

## Generic Relationship Manager (RelationshipManagerBase)

All bounded contexts should use the **generic `RelationshipManagerBase<TEntity, TRelatedEntity, TJunctionEntity>`** class located in `Inventorization.Base.Services` for implementing many-to-many relationship managers. This eliminates boilerplate relationship management code while enforcing consistent patterns.

### Creating Relationship Managers in a Bounded Context

Each many-to-many relationship requires:

#### 1. Junction Entity (in BoundedContext.BL/Entities)

Junction entities should inherit from `JunctionEntityBase` which provides standard `EntityId` and `RelatedEntityId` properties:

```csharp
using Inventorization.Base.Models;

public class UserRole : JunctionEntityBase
{
    public UserRole(Guid userId, Guid roleId) : base(userId, roleId)
    {
    }

    // Property aliases for semantic clarity
    public Guid UserId => EntityId;
    public Guid RoleId => RelatedEntityId;
    
    // Navigation properties
    public User User { get; } = null!;
    public Role Role { get; } = null!;
}
```

**Benefits of JunctionEntityBase:**
- Eliminates parameterless constructor boilerplate
- Automatic validation of Guid.Empty
- Standardized EntityId/RelatedEntityId properties
- Reduces junction entity code by ~50%

#### 2. Property Accessors (in BoundedContext.BL/PropertyAccessors)

Property accessors encapsulate property access logic and are resolved via dependency injection:

```csharp
using Inventorization.Base.Abstractions;

public class UserRoleEntityIdPropertyAccessor 
    : PropertyAccessor<UserRole, Guid>, IEntityIdPropertyAccessor<UserRole>
{
    public UserRoleEntityIdPropertyAccessor() : base(ur => ur.UserId) { }
}

public class UserRoleRelatedEntityIdPropertyAccessor 
    : PropertyAccessor<UserRole, Guid>, IRelatedEntityIdPropertyAccessor<UserRole>
{
    public UserRoleRelatedEntityIdPropertyAccessor() : base(ur => ur.RoleId) { }
}
```

#### 3. Concrete Relationship Manager (in BoundedContext.BL/DataServices)

**Step-by-step example** for a `User ↔ Role` relationship:

```csharp
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
using Inventorization.Base.Abstractions;
using Microsoft.Extensions.Logging;

public class UserRoleRelationshipManager 
    : RelationshipManagerBase<User, Role, UserRole>
{
    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger,
        IRelationshipMetadata<User, Role> metadata)  // Injected from DI
        : base(userRepository, roleRepository, userRoleRepository, unitOfWork, serviceProvider, logger, metadata)
    {
    }
    
    // That's it! Metadata injected via DI from DataModelRelationships.
    // Property accessors resolved from DI automatically.
    // CreateJunctionEntity uses reflection by default.
}
```

**That's it!** Only ~24 lines of code vs ~200 lines without the base class. All relationship logic is inherited, and property accessors are resolved from DI.

### RelationshipManagerBase Features

The `RelationshipManagerBase` class provides:

- ✅ **Complete implementation** of `IRelationshipManager<TEntity, TRelatedEntity>` interface
- ✅ **Entity existence validation** before updating relationships
- ✅ **DTO validation** using `IValidator<EntityReferencesDTO>` resolved at runtime
- ✅ **Add/Remove operations** with automatic duplicate detection
- ✅ **Transaction management** with automatic `SaveChangesAsync()` calls
- ✅ **Comprehensive logging** with dynamic entity type names
- ✅ **Bulk operations** with aggregated results and error tracking
- ✅ **Expression-based queries** for optimal EF Core translation

### Abstraction Points

The base class resolves dependencies and provides extension points:

1. **EntityIdAccessor**: Property accessor for parent entity ID (resolved from DI)
   - Type: `IPropertyAccessor<TJunctionEntity, Guid>`
   - Resolved via: `IEntityIdPropertyAccessor<TJunctionEntity>`
   - Provides expression for LINQ queries and compiled getter for performance
   - Automatically resolved in `RelationshipManagerBase` constructor

2. **RelatedEntityIdAccessor**: Property accessor for related entity ID (resolved from DI)
   - Type: `IPropertyAccessor<TJunctionEntity, Guid>`
   - Resolved via: `IRelatedEntityIdPropertyAccessor<TJunctionEntity>`
   - Provides expression for LINQ queries and compiled getter for performance
   - Automatically resolved in `RelationshipManagerBase` constructor

3. **CreateJunctionEntity**: Factory function for instantiating junction entities (optional override)
   - Type: `Func<Guid, Guid, TJunctionEntity>`
   - Default: Uses reflection to call two-parameter constructor
   - Override only if custom instantiation logic is required

### Dependency Resolution Pattern

`RelationshipManagerBase` constructor (all injected via DI):

```csharp
IRepository<TEntity> entityRepository,                // Parent entity repository
IRepository<TRelatedEntity> relatedEntityRepository,  // Related entity repository
IRepository<TJunctionEntity> junctionRepository,      // Junction entity repository
IUnitOfWork unitOfWork,                               // Transaction management
IServiceProvider serviceProvider,                     // For runtime resolution
ILogger logger                                        // Logging
```

**Constructor-time resolution:**
- `IEntityIdPropertyAccessor<TJunctionEntity>` - Resolved in constructor
- `IRelatedEntityIdPropertyAccessor<TJunctionEntity>` - Resolved in constructor

**Runtime resolution:**
- `IValidator<EntityReferencesDTO>` - Resolved when validation is needed
- Should validate that related entity IDs exist and business rules are met

### Dependency Injection Registration

All relationship management components must be registered in DI (typically in `Program.cs`):

```csharp
// 1. Register junction entity repository
builder.Services.AddScoped<IRepository<UserRole>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new BaseRepository<UserRole>(dbContext);
});

// 2. Register property accessors for the junction entity
builder.Services.AddScoped<IEntityIdPropertyAccessor<UserRole>, UserRoleEntityIdPropertyAccessor>();
builder.Services.AddScoped<IRelatedEntityIdPropertyAccessor<UserRole>, UserRoleRelatedEntityIdPropertyAccessor>();

// 3. Register relationship metadata with keyed service
builder.Services.AddKeyedSingleton<IRelationshipMetadata<User, Role>>(
    "UserRoles",
    (sp, key) => DataModelRelationships.UserRoles);

// 4. Register relationship manager with metadata injection
builder.Services.AddScoped<IRelationshipManager<User, Role>>(sp =>
    new UserRoleRelationshipManager(
        sp.GetRequiredService<IRepository<User>>(),
        sp.GetRequiredService<IRepository<Role>>(),
        sp.GetRequiredService<IRepository<UserRole>>(),
        sp.GetRequiredService<IUnitOfWork>(),
        sp,
        sp.GetRequiredService<ILogger<UserRoleRelationshipManager>>(),
        sp.GetRequiredKeyedService<IRelationshipMetadata<User, Role>>("UserRoles")));

// 5. Register validator
builder.Services.AddScoped<IValidator<EntityReferencesDTO>, EntityReferencesValidator>();
```

**Important:** 
- **Property accessors** must be registered for every junction entity type
- **Relationship metadata** must be registered with keyed services from `DataModelRelationships`
- **Keyed service pattern** (`.NET 8+`) allows multiple relationships to the same entity type
- This is a **required** part of the bounded context boilerplate

### Benefits

- **88%+ code reduction**: ~24 lines instead of ~200 lines
- **Centralized metadata**: Single source of truth in `DataModelRelationships` static class
- **Dependency injection**: Property accessors and metadata resolved via DI
- **Consistency**: All relationship managers follow identical patterns
- **Type safety**: Expression-based approach provides compile-time validation
- **Performance**: Expressions compile to efficient EF Core SQL queries, getters are cached
- **Maintainability**: Business logic changes only need to be made once in base class
- **Testability**: Mock/stub property accessors and relationship managers independently
- **Reusability**: Property accessors can be used by mappers, validators, and other components
- **Multiple relationships**: Keyed services enable multiple relationships between same entity types

---

## Generic One-to-Many Relationship Manager (OneToManyRelationshipManagerBase)

All bounded contexts should use the **generic `OneToManyRelationshipManagerBase<TParent, TChild>`** class for managing one-to-many relationships where children have a foreign key to the parent.

### Creating One-to-Many Managers in a Bounded Context

Each one-to-many relationship requires:

#### 1. Child Entity with Foreign Key (in BoundedContext.BL/Entities)

```csharp
using Inventorization.Base.Models;

public class RefreshToken : BaseEntity
{
    private RefreshToken() { } // EF Core only
    
    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required");
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required");
        
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    
    // Navigation property
    public User User { get; } = null!;
    
    public void UpdateUserId(Guid userId)
    {
        UserId = userId;
    }
}
```

#### 2. Property Accessor (in BoundedContext.BL/PropertyAccessors)

```csharp
using Inventorization.Base.Abstractions;

public class RefreshTokenUserIdAccessor 
    : PropertyAccessor<RefreshToken, Guid>
{
    public RefreshTokenUserIdAccessor() : base(rt => rt.UserId) { }
}
```

#### 3. Concrete Relationship Manager (in BoundedContext.BL/DataServices)

```csharp
using Inventorization.Base.Services;
using Inventorization.Base.Abstractions;
using Microsoft.Extensions.Logging;

public class UserRefreshTokenRelationshipManager 
    : OneToManyRelationshipManagerBase<User, RefreshToken>
{
    public UserRefreshTokenRelationshipManager(
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRefreshTokenRelationshipManager> logger,
        IRelationshipMetadata<User, RefreshToken> metadata,  // Injected from DI
        Type parentIdAccessorType)
        : base(
            userRepository,
            refreshTokenRepository,
            unitOfWork,
            serviceProvider,
            logger,
            metadata,
            parentIdAccessorType)
    {
    }

    protected override void SetParentId(RefreshToken child, Guid? parentId)
    {
        if (parentId == null)
            throw new InvalidOperationException("Cannot set UserId to null on RefreshToken");
        
        child.UpdateUserId(parentId.Value);
    }
}
```

**Only ~40 lines of code!** Parent-child relationship fully managed with add/remove/replace operations.

### OneToManyRelationshipManagerBase Features

- ✅ **GetChildIdsAsync** - Retrieve all children for a parent
- ✅ **AddChildAsync** - Associate a child with a parent
- ✅ **RemoveChildAsync** - Dissociate a child from a parent (if cardinality allows)
- ✅ **ReplaceChildrenAsync** - Replace entire child collection
- ✅ **Cardinality validation** - Prevents removing required relationships
- ✅ **Comprehensive logging** - All operations logged with entity names
- ✅ **Transaction management** - Automatic SaveChangesAsync calls

### Dependency Injection Registration

```csharp
// 1. Register child entity repository
builder.Services.AddScoped<IRepository<RefreshToken>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new BaseRepository<RefreshToken>(dbContext);
});

// 2. Register property accessor
builder.Services.AddScoped<RefreshTokenUserIdAccessor>();

// 3. Register relationship metadata with keyed service
builder.Services.AddKeyedSingleton<IRelationshipMetadata<User, RefreshToken>>(
    "UserRefreshTokens",
    (sp, key) => DataModelRelationships.UserRefreshTokens);

// 4. Register relationship manager with metadata injection
builder.Services.AddScoped<IOneToManyRelationshipManager<User, RefreshToken>>(sp =>
    new UserRefreshTokenRelationshipManager(
        sp.GetRequiredService<IRepository<User>>(),
        sp.GetRequiredService<IRepository<RefreshToken>>(),
        sp.GetRequiredService<IUnitOfWork>(),
        sp,
        sp.GetRequiredService<ILogger<UserRefreshTokenRelationshipManager>>(),
        sp.GetRequiredKeyedService<IRelationshipMetadata<User, RefreshToken>>("UserRefreshTokens"),
        typeof(RefreshTokenUserIdAccessor)));
```

---

## Generic One-to-One Relationship Manager (OneToOneRelationshipManagerBase)

All bounded contexts should use the **generic `OneToOneRelationshipManagerBase<TEntity, TRelatedEntity>`** class for managing one-to-one relationships.

### Creating One-to-One Managers in a Bounded Context

Each one-to-one relationship requires:

#### 1. Entity with Foreign Key (in BoundedContext.BL/Entities)

```csharp
using Inventorization.Base.Models;

public class User : BaseEntity
{
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public Guid? UserProfileId { get; private set; } // Optional FK
    
    // Navigation property
    public UserProfile? UserProfile { get; private set; }
    
    public void SetProfile(Guid? profileId)
    {
        UserProfileId = profileId;
    }
}
```

#### 2. Property Accessor (in BoundedContext.BL/PropertyAccessors)

```csharp
using Inventorization.Base.Abstractions;

public class UserProfileIdAccessor 
    : PropertyAccessor<User, Guid?>
{
    public UserProfileIdAccessor() : base(u => u.UserProfileId) { }
}
```

#### 3. Concrete Relationship Manager (in BoundedContext.BL/DataServices)

```csharp
using Inventorization.Base.Services;
using Inventorization.Base.Abstractions;
using Microsoft.Extensions.Logging;

public class UserProfileRelationshipManager 
    : OneToOneRelationshipManagerBase<User, UserProfile>
{
    public UserProfileRelationshipManager(
        IRepository<User> userRepository,
        IRepository<UserProfile> userProfileRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserProfileRelationshipManager> logger,
        IRelationshipMetadata<User, UserProfile> metadata,  // Injected from DI
        Type relatedIdAccessorType)
        : base(
            userRepository,
            userProfileRepository,
            unitOfWork,
            serviceProvider,
            logger,
            metadata,
            relatedIdAccessorType)
    {
    }

    protected override void SetRelatedId(User entity, Guid? relatedId)
    {
        entity.SetProfile(relatedId);
    }
}
```

**Only ~35 lines of code!** One-to-one relationship fully managed with get/set/remove operations.

### OneToOneRelationshipManagerBase Features

- ✅ **GetRelatedIdAsync** - Get the related entity ID
- ✅ **SetRelatedEntityAsync** - Set or update the related entity
- ✅ **RemoveRelationshipAsync** - Remove the relationship (if cardinality allows)
- ✅ **Cardinality validation** - Prevents removing required relationships
- ✅ **Existence validation** - Ensures both entities exist before creating relationship
- ✅ **Comprehensive logging** - All operations logged with entity names
- ✅ **Transaction management** - Automatic SaveChangesAsync calls

### Dependency Injection Registration

```csharp
// 1. Register related entity repository
builder.Services.AddScoped<IRepository<UserProfile>>(sp =>
{
    var dbContext = sp.GetRequiredService<AuthDbContext>();
    return new BaseRepository<UserProfile>(dbContext);
});

// 2. Register property accessor
builder.Services.AddScoped<UserProfileIdAccessor>();

// 3. Register relationship metadata with keyed service (if UserProfile relationship defined)
builder.Services.AddKeyedSingleton<IRelationshipMetadata<User, UserProfile>>(
    "UserProfiles",
    (sp, key) => DataModelRelationships.UserProfiles); // Add to DataModelRelationships if needed

// 4. Register relationship manager with metadata injection
builder.Services.AddScoped<IOneToOneRelationshipManager<User, UserProfile>>(sp =>
    new UserProfileRelationshipManager(
        sp.GetRequiredService<IRepository<User>>(),
        sp.GetRequiredService<IRepository<UserProfile>>(),
        sp.GetRequiredService<IUnitOfWork>(),
        sp,
        sp.GetRequiredService<ILogger<UserProfileRelationshipManager>>(),
        sp.GetRequiredKeyedService<IRelationshipMetadata<User, UserProfile>>("UserProfiles"),
        typeof(UserProfileIdAccessor)));
```

---

## Relationship Type Decision Matrix

Use this matrix to choose the appropriate relationship manager:

| Relationship Pattern | Manager Base Class | When to Use | Example |
|---------------------|-------------------|-------------|---------|
| **Many-to-Many** | `RelationshipManagerBase` | Multiple entities can relate to multiple entities | User ↔ Role, Role ↔ Permission, Student ↔ Course |
| **One-to-Many** | `OneToManyRelationshipManagerBase` | One parent has multiple children | User → RefreshTokens, Category → Products, Order → OrderItems |
| **One-to-One** | `OneToOneRelationshipManagerBase` | One entity relates to exactly one other entity | User ↔ UserProfile, Order ↔ ShippingAddress |

### Cardinality Guidelines

| Cardinality | Use Case | Can Remove? | Example |
|-------------|----------|-------------|---------|
| **Required** | Child cannot exist without parent | ❌ No | OrderItem → Order, RefreshToken → User |
| **Optional** | Child can exist independently | ✅ Yes | User → UserProfile, Product → Category |

### Property Accessor Patterns

All relationship managers use property accessors for type-safe, reusable property access:

```csharp
// For junction entities (ManyToMany)
IEntityIdPropertyAccessor<TJunctionEntity>        // Parent entity ID
IRelatedEntityIdPropertyAccessor<TJunctionEntity> // Related entity ID

// For child entities (OneToMany)
IPropertyAccessor<TChild, Guid>                   // Parent foreign key

// For entities with FK (OneToOne)
IPropertyAccessor<TEntity, Guid?>                 // Related entity FK (nullable)
```

---

## Bounded Context Boilerplate Checklist

### Project-Level Setup (Required for All Bounded Contexts)

✅ **DI Project**: `Inventorization.[BoundedContextName].DI` (generated)
   - `Extensions/[ContextName]ServiceCollectionExtensions.cs` with all register logic
   - References: Base, DTO, Domain, Common projects
   
✅ **Test Project**: `Inventorization.[BoundedContextName].API.Tests` (generated)
   - Folder structure: Services/, Validators/, Mappers/, SearchProviders/, Creators/, Modifiers/, Instantiation/
   - 211+ tests per entity set (data service, validator, mapper, search, creator, modifier, instantiation)
   - Uses xUnit + FluentAssertions + Moq + EF Core InMemory
   
✅ **Program.cs in API Project** (generated)
   - Adds all framework services and calls `builder.Services.Add[ContextName]Services(configuration)`
   - DI extension integration complete

### Base Infrastructure (Required for All Bounded Contexts)

✅ **DbContext** using `ApplyConfigurationsFromAssembly()` (keep under 50 lines)  
✅ **EntityConfigurations folder** with one `IEntityTypeConfiguration<T>` per entity  
✅ **DataModelRelationships.cs** static class with all relationship metadata  
✅ **UnitOfWork** inheriting from `UnitOfWorkBase<TDbContext>`  
✅ **DTOs** for each entity (Create, Update, Delete, Details, Search)  

### For Many-to-Many Relationships

✅ **Junction entity** inheriting from `JunctionEntityBase`  
✅ **Junction configuration** inheriting from `JunctionEntityConfiguration<TJunction, TEntity, TRelated>`  
✅ **Relationship metadata** in `DataModelRelationships` static class  
✅ **Two property accessors**: `IEntityIdPropertyAccessor<TJunction>`, `IRelatedEntityIdPropertyAccessor<TJunction>`  
✅ **Relationship manager** inheriting from `RelationshipManagerBase`  
✅ **DI registrations**: 
   - Junction repository
   - Property accessors
   - Relationship metadata (keyed service)
   - Relationship manager (with metadata injection)
   - Validator
   
✅ **Generated Tests**:
   - Many-to-many entity instantiation tests verify composite key behavior
   - Relationship manager tests verify add/remove operations

### For One-to-Many Relationships

✅ **Child entity** with parent foreign key  
✅ **Child entity configuration** inheriting from `BaseEntityConfiguration<TChild>`  
✅ **Relationship metadata** in `DataModelRelationships` static class  
✅ **One property accessor**: `IPropertyAccessor<TChild, Guid>` for parent ID  
✅ **Relationship manager** inheriting from `OneToManyRelationshipManagerBase`  
✅ **SetParentId method** implementation in manager  
✅ **DI registrations**: 
   - Child repository
   - Property accessor
   - Relationship metadata (keyed service)
   - Relationship manager (with metadata injection)

✅ **Generated Tests**:
   - Child entity instantiation tests verify FK behavior

### For One-to-One Relationships

✅ **Entity** with related entity foreign key (nullable if optional)  
✅ **Entity configuration** inheriting from `BaseEntityConfiguration<TEntity>`  
✅ **Relationship metadata** in `DataModelRelationships` static class (if managed)  
✅ **One property accessor**: `IPropertyAccessor<TEntity, Guid?>` for related ID  
✅ **Relationship manager** inheriting from `OneToOneRelationshipManagerBase` (if managed)  
✅ **SetRelatedId method** implementation in manager  
✅ **DI registrations**: 
   - Related repository
   - Property accessor
   - Relationship metadata (keyed service)
   - Relationship manager (with metadata injection)

### For Regular Entities (No Relationships)

✅ **Entity** inheriting from `BaseEntity`  
✅ **Entity configuration** inheriting from `BaseEntityConfiguration<TEntity>`  
✅ **Data service** inheriting from `DataServiceBase<...>`  
✅ **Creator, Modifier, Mapper, SearchProvider** implementations  
✅ **Validators** for Create and Update DTOs  
✅ **DI registrations**: Repository, data service, abstractions, validators  

---

### Search Abstraction
- `SearchDTO` base class includes:
  - `PageDTO` for pagination
  - Abstract generic properties: `FilterDTO`, `ProjectionDTO` (implemented in each concrete `SearchDTO`)
  - Optionally, add `SortDTO` for sorting

### Dependency Injection Registration

In your bounded context's DI setup (typically Program.cs or an extension method):

```csharp
// DTOs
builder.Services.AddScoped<IMapper<Customer, CustomerDetailsDTO>, CustomerMapper>();
builder.Services.AddScoped<IEntityCreator<Customer, CreateCustomerDTO>, CustomerCreator>();
builder.Services.AddScoped<IEntityModifier<Customer, UpdateCustomerDTO>, CustomerModifier>();
builder.Services.AddScoped<ISearchQueryProvider<Customer, CustomerSearchDTO>, CustomerSearchProvider>();

// Validators
builder.Services.AddScoped<IValidator<CreateCustomerDTO>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDTO>, UpdateCustomerValidator>();

// Data Service
builder.Services.AddScoped<ICustomerDataService, CustomerDataService>();
builder.Services.AddScoped(typeof(IUnitOfWork), sp => sp.GetRequiredService<IBoundedContextUnitOfWork>());
builder.Services.AddScoped(typeof(IRepository<>), typeof(EntityFrameworkRepository<>));
```

### All DataService Implementations Must Use This Pattern

All DataService implementations must:
- Inherit from `DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>`
- Be injected as their interface (e.g., `ICustomerDataService` or `IDataService<T...>`), never as concrete type
- Have minimal concrete code (typically just a constructor)
- Rely on inherited CRUD/Search logic from base class

---

## Entity Relationship Management Patterns

This section defines patterns for managing entity relationships (one-to-many, many-to-many) with explicit guidance on when to use entity methods, dedicated services, or API endpoints.

### Three-Tier Relationship Strategy

**Tier 1: Simple Associations (Parent-Managed)**
- Junction table with no additional metadata
- Managed via parent entity methods (`AssignRole()`, `RevokeRole()`) AND API endpoints
- No dedicated DataService for junction entity
- No dedicated Controller for junction entity
- Uses `IRelationshipManager<TEntity, TRelatedEntity>` for API persistence layer

**When to use:**
- Pure many-to-many relationships (UserRole, RolePermission)
- No additional columns beyond foreign keys
- Simple business rules (no duplicates, entity exists)

**Example:** UserRole junction (User ↔ Role)

---

**Tier 2: Query-Only Services**
- Relationships require complex queries/filtering
- Custom service interface (NOT `IDataService`)
- Read-only operations via specialized methods
- Optional HTTP endpoints (GET only) for queries
- No CRUD controllers

**When to use:**
- Complex relationship queries (graph traversal, aggregations)
- Permission checking (user has permission through role chain)
- Reporting (count relationships, find indirect associations)

**Example:** RolePermissionService (queries permissions through role hierarchy)

---

**Tier 3: Full CRUD Junction Entities**
- Relationships have additional metadata (timestamps, notes, status, quantity)
- Full DTO set (Create, Update, Delete, Details, Search)
- Extends `DataServiceBase<...>`
- Dedicated `DataController<...>` with CRUD endpoints
- Junction entity gets its own API surface

**When to use:**
- Junction entity has business-meaningful metadata
- Audit trail required (who added relationship and when)
- Soft delete needed (reversible relationships)
- Relationships are first-class domain entities

**Example:** ProjectMembership (User ↔ Project with JoinDate, Role, Status)

---

### Decision Matrix

| Criteria | Tier 1 (Simple) | Tier 2 (Query-Only) | Tier 3 (Full CRUD) |
|----------|----------------|---------------------|-------------------|
| **Metadata** | FK only | FK only | FK + additional columns |
| **Business Logic** | Basic validation | Complex queries | Full CRUD + validation |
| **Audit Requirements** | MongoDB audit log | MongoDB audit log | Entity audit columns + MongoDB |
| **API Endpoints** | PATCH relationships | GET queries only | Full CRUD endpoints |
| **Entity Methods** | Yes | No | Optional |
| **DataService** | `RelationshipManagerBase` | Custom interface | `DataServiceBase` |
| **Controller** | Parent controller | Optional custom | Dedicated `DataController` |

---

### Junction Entity Implementation Rules

All junction entities must follow these rules:

**1. Naming Convention:** `{ParentEntity}x{ChildEntity}` or `{ParentEntity}{ChildEntity}`
```csharp
UserRole   // User ↔ Role
BxC        // B ↔ C (abbreviated when names are short)
```

**2. Immutability Pattern:** Private setters, parameterized constructor
```csharp
public class UserRole
{
    private UserRole() { }  // EF Core only
    
    public UserRole(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID required");
        if (roleId == Guid.Empty) throw new ArgumentException("Role ID required");
        
        UserId = userId;
        RoleId = roleId;
    }
    
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
}
```

**3. Composite Primary Key:** Configure in DbContext via Fluent API
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UserRole>()
        .HasKey(ur => new { ur.UserId, ur.RoleId });
    
    // Foreign key relationships with CASCADE DELETE PROHIBITED
    modelBuilder.Entity<UserRole>()
        .HasOne(ur => ur.User)
        .WithMany(u => u.UserRoles)
        .HasForeignKey(ur => ur.UserId)
        .OnDelete(DeleteBehavior.Restrict);  // REQUIRED
    
    modelBuilder.Entity<UserRole>()
        .HasOne(ur => ur.Role)
        .WithMany(r => r.UserRoles)
        .HasForeignKey(ur => ur.RoleId)
        .OnDelete(DeleteBehavior.Restrict);  // REQUIRED
}
```

---

### Soft Delete Strategy

For relationships requiring audit trails or reversible deletion, implement `ISoftDeletableEntity` on the junction entity:

**When to use soft delete:**
- Regulatory compliance requires relationship history
- Relationships may be temporarily disabled and re-enabled
- Audit trail must show who deleted relationship and when

**Implementation:**
```csharp
public class ProjectMembership : ISoftDeletableEntity
{
    private ProjectMembership() { }
    
    public ProjectMembership(Guid projectId, Guid userId, string role)
    {
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
    
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = null!;
    public DateTime JoinedAt { get; private set; }
    
    // ISoftDeletableEntity implementation
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
```

**DbContext configuration with query filter:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global query filter to exclude soft-deleted entities
    modelBuilder.Entity<ProjectMembership>()
        .HasQueryFilter(pm => !pm.IsDeleted);
    
    // To include soft-deleted: context.ProjectMemberships.IgnoreQueryFilters()
}
```

**When NOT to use soft delete:**
- Simple associations with no audit requirements
- High-volume relationships (performance concern with query filters)
- MongoDB audit logging sufficient for compliance

---

### Transaction Responsibility

Clear separation of transaction handling across layers:

**Entity Mutation Methods:**
- Modify navigation collections directly
- **DO NOT** call `SaveChangesAsync()`
- Caller responsible for persistence
- Example:
```csharp
public void AssignRole(Role role)
{
    UserRoles.Add(new UserRole(Id, role.Id));
    // No SaveChangesAsync() here
}

// Caller handles transaction
var user = await userRepository.GetByIdAsync(userId);
user.AssignRole(role);
await unitOfWork.SaveChangesAsync();  // Caller's responsibility
```

**IRelationshipManager Implementations:**
- **ALWAYS inherit from `RelationshipManagerBase<TEntity, TRelatedEntity, TJunctionEntity>`**
- Base class handles validation, repository operations, AND transactions
- Base class **automatically** calls `unitOfWork.SaveChangesAsync()` after all changes
- Concrete implementations only define three abstract properties:
  - `CreateJunctionEntity` - Factory function for junction entity instantiation
  - `EntityIdSelector` - Expression to extract parent entity ID from junction
  - `RelatedEntityIdSelector` - Expression to extract related entity ID from junction

**Example:**
```csharp
public class UserRoleRelationshipManager 
    : RelationshipManagerBase<User, Role, UserRole>
{
    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger)
        : base(userRepository, roleRepository, userRoleRepository, unitOfWork, serviceProvider, logger)
    {
    }

    protected override Func<Guid, Guid, UserRole> CreateJunctionEntity => 
        (userId, roleId) => new UserRole(userId, roleId);

    protected override Expression<Func<UserRole, Guid>> EntityIdSelector => 
        ur => ur.UserId;

    protected override Expression<Func<UserRole, Guid>> RelatedEntityIdSelector => 
        ur => ur.RoleId;
}
```

**What RelationshipManagerBase Provides:**
- Generic implementation of all three `IRelationshipManager` methods
- Entity existence validation
- DTO validation via `IValidator<EntityReferencesDTO>`
- Add/Remove operations with duplicate detection
- Transaction management (calls `SaveChangesAsync`)
- Comprehensive logging with entity type names
- Bulk operations with aggregated results
- Error handling and recovery

**API Controllers:**
- Delegate to `IRelationshipManager`
- **DO NOT** call `SaveChangesAsync()` directly
- Rely on manager's transaction handling

---

### Cascade Delete Policy

**REQUIRED: All foreign key relationships MUST be configured with `DeleteBehavior.Restrict`**

**Rationale:** Automatic cascade deletion is EXTREMELY DANGEROUS and can lead to accidental data loss. All deletions must be explicit and validated.

**EF Core Configuration (REQUIRED):**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // CORRECT: Cascade deletion PROHIBITED
    modelBuilder.Entity<UserRole>()
        .HasOne(ur => ur.User)
        .WithMany(u => u.UserRoles)
        .HasForeignKey(ur => ur.UserId)
        .OnDelete(DeleteBehavior.Restrict);  // ✅ REQUIRED
    
    // INCORRECT: Never use cascade deletion
    // .OnDelete(DeleteBehavior.Cascade);  // ❌ PROHIBITED
}
```

**Manual Deletion Pattern:**
```csharp
public async Task<ServiceResult<bool>> DeleteUserAsync(Guid userId)
{
    var user = await _repository.GetByIdAsync(userId);
    if (user == null)
        return ServiceResult<bool>.Failure("User not found");
    
    // Check for dependent relationships
    var roleCount = user.UserRoles.Count;
    if (roleCount > 0)
    {
        return ServiceResult<bool>.Failure(
            $"Cannot delete user - assigned to {roleCount} roles. Remove roles first.");
    }
    
    // Safe to delete
    await _repository.DeleteAsync(user);
    await _unitOfWork.SaveChangesAsync();
    
    return ServiceResult<bool>.Success(true);
}
```

---

### Validation Strategy

Use `IValidator<EntityReferencesDTO>` for validating relationship changes before applying:

```csharp
public class EntityReferencesValidator : IValidator<EntityReferencesDTO>
{
    private readonly IRepository<Role> _roleRepository;
    
    public EntityReferencesValidator(IRepository<Role> roleRepository)
    {
        _roleRepository = roleRepository;
    }
    
    public async Task<ValidationResult> ValidateAsync(
        EntityReferencesDTO dto, 
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        
        // Validate entities to add exist
        if (dto.IdsToAdd.Any())
        {
            foreach (var roleId in dto.IdsToAdd)
            {
                var exists = await _roleRepository.ExistsAsync(roleId, cancellationToken);
                if (!exists)
                    errors.Add($"Role {roleId} not found");
            }
        }
        
        // Business rules
        if (dto.IdsToAdd.Count > 10)
            errors.Add("Cannot assign more than 10 roles at once");
        
        return errors.Any() 
            ? ValidationResult.WithErrors(errors.ToArray()) 
            : ValidationResult.Ok();
    }
}
```

**Integration in IRelationshipManager:**
```csharp
public async Task<RelationshipUpdateResult> UpdateRelationshipsAsync(...)
{
    // Validate first
    var validator = _serviceProvider.GetRequiredService<IValidator<EntityReferencesDTO>>();
    var validationResult = await validator.ValidateAsync(changes, cancellationToken);
    
    if (!validationResult.IsValid)
        return RelationshipUpdateResult.Failure("Validation failed", validationResult.Errors);
    
    // Proceed with update
    // ...
}
```

---

### Bulk Operations

For performance-critical scenarios with many relationships (50+ entities), use bulk update methods:

**IRelationshipManager Interface:**
```csharp
Task<BulkRelationshipUpdateResult> UpdateMultipleRelationshipsAsync(
    Dictionary<Guid, EntityReferencesDTO> changes,
    CancellationToken cancellationToken);
```

**Implementation Pattern:**
```csharp
public async Task<BulkRelationshipUpdateResult> UpdateMultipleRelationshipsAsync(
    Dictionary<Guid, EntityReferencesDTO> changes,
    CancellationToken cancellationToken)
{
    var results = new Dictionary<Guid, RelationshipUpdateResult>();
    int totalAdded = 0;
    int totalRemoved = 0;
    int successful = 0;
    int failed = 0;
    var errors = new List<string>();
    
    foreach (var (entityId, entityChanges) in changes)
    {
        try
        {
            var result = await UpdateRelationshipsAsync(entityId, entityChanges, cancellationToken);
            results[entityId] = result;
            
            if (result.IsSuccess)
            {
                totalAdded += result.AddedCount;
                totalRemoved += result.RemovedCount;
                successful++;
            }
            else
            {
                failed++;
                errors.AddRange(result.Errors);
            }
        }
        catch (Exception ex)
        {
            failed++;
            errors.Add($"Entity {entityId}: {ex.Message}");
            results[entityId] = RelationshipUpdateResult.Failure(ex.Message);
        }
    }
    
    if (failed == 0)
        return BulkRelationshipUpdateResult.Success(totalAdded, totalRemoved, successful, results);
    else
        return BulkRelationshipUpdateResult.PartialSuccess(totalAdded, totalRemoved, successful, failed, results, errors);
}
```

**Performance Recommendations:**
- Batch operations in chunks of 100-500 entities per transaction
- Use `EF.CompileAsyncQuery()` for hot paths
- Consider background jobs (Hangfire) for operations > 1000 entities

---

### Complete Example: Tier 1 Implementation (User ↔ Role)

**1. Junction Entity**
```csharp
public class UserRole
{
    private UserRole() { }
    
    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
    
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
}
```

**2. Entity Mutation Methods**
```csharp
public class User
{
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    
    public void AssignRole(Role role)
    {
        if (HasRole(role.Id)) return;
        UserRoles.Add(new UserRole(Id, role.Id));
    }
    
    public void RevokeRole(Guid roleId)
    {
        var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null) UserRoles.Remove(userRole);
    }
    
    public bool HasRole(Guid roleId) => UserRoles.Any(ur => ur.RoleId == roleId);
}
```

**3. IRelationshipManager Implementation**
```csharp
public class UserRoleRelationshipManager 
    : RelationshipManagerBase<User, Role, UserRole>
{
    public UserRoleRelationshipManager(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider,
        ILogger<UserRoleRelationshipManager> logger)
        : base(userRepository, roleRepository, userRoleRepository, unitOfWork, serviceProvider, logger)
    {
    }

    // Define junction entity factory
    protected override Func<Guid, Guid, UserRole> CreateJunctionEntity => 
        (userId, roleId) => new UserRole(userId, roleId);

    // Define how to extract parent entity ID from junction
    protected override Expression<Func<UserRole, Guid>> EntityIdSelector => 
        ur => ur.UserId;

    // Define how to extract related entity ID from junction
    protected override Expression<Func<UserRole, Guid>> RelatedEntityIdSelector => 
        ur => ur.RoleId;
}
```

**Note:** The base class `RelationshipManagerBase<TEntity, TRelatedEntity, TJunctionEntity>` provides complete implementation of all three interface methods. Concrete classes only define the three abstract properties above (~30 lines total vs ~200 lines without base class).

**4. Controller Implementation**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : 
    DataController<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, UserDetailsDTO, UserSearchDTO, IUserDataService>,
    IRelationController<Role>
{
    private readonly UserRoleRelationHandler _roleHandler;
    
    public UsersController(
        IUserDataService dataService,
        IRelationshipManager<User, Role> roleRelationshipManager,
        ILogger<UsersController> logger)
        : base(dataService, logger)
    {
        _roleHandler = new UserRoleRelationHandler(roleRelationshipManager, logger);
    }
    
    [HttpPatch("{id}/relationships/roles")]
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Role>.UpdateRelationshipsAsync(
        Guid id,
        EntityReferencesDTO changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateRelationshipsAsync(id, changes, "Roles", cancellationToken);
    }
    
    [HttpPatch("relationships/roles/bulk")]
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> IRelationController<Role>.UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateMultipleRelationshipsAsync(changes, "Roles", cancellationToken);
    }
    
    private class UserRoleRelationHandler : DataRelationHandler<User, Role>
    {
        public UserRoleRelationHandler(IRelationshipManager<User, Role> manager, ILogger logger)
            : base(manager, logger) { }
    }
}
```

**5. DI Registration**
```csharp
builder.Services.AddScoped<IRelationshipManager<User, Role>, UserRoleRelationshipManager>();
builder.Services.AddScoped<IValidator<EntityReferencesDTO>, EntityReferencesValidator>();
```

---

### CQRS (Advanced)
- For complex domains, consider separating read/write services (CQRS).

### Documentation
- Document all abstractions and expected extension points in code and markdown.

---

## Controller Architecture Patterns

All API controllers must follow an abstract generic base class pattern to eliminate code duplication and maintain consistency. See [CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md) for complete guidance.

### Project Structure
- **Inventorization.[BoundedContextName].API.Base** - Shared abstract controllers (non-generic and generic)
  - `Controllers/ServiceController.cs` - Non-generic base extending `ControllerBase`
  - `Controllers/DataController.cs` - Generic CRUD base extending `ServiceController`
    - `Controllers/BaseQueryController.cs` - Generic ADT query base for complex search/projections
- **Inventorization.[BoundedContextName].API** - Concrete controllers
  - References `...API.Base` project
  - All controllers inherit from generic base classes
  - Folder structure mirrors base controller types:
    - `Controllers/Data/` - Concrete controllers extending `DataController<...>`
        - `Controllers/` - `{Entity}sQueryController` extending `BaseQueryController<...>` for complex search
    - `Controllers/[CustomType]/` - Other specialized controllers as needed

### ServiceController
Abstract non-generic base class providing common controller functionality:
```csharp
public abstract class ServiceController : ControllerBase
{
    protected readonly ILogger<ServiceController> Logger;
    
    protected ServiceController(ILogger<ServiceController> logger)
    {
        Logger = logger;
    }
}
```

### DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO, TService>
Abstract generic base class extending `ServiceController` providing all CRUD operations:
- `GET /{id}` → `GetByIdAsync(id)` - Returns `ServiceResult<TDetailsDTO>`
- `POST /` → `CreateAsync(dto)` - Returns `ServiceResult<TDetailsDTO>` with 201 Created
- `PUT /{id}` → `UpdateAsync(id, dto)` - Returns `ServiceResult<TDetailsDTO>`
- `DELETE /{id}` → `DeleteAsync(id)` - Returns `ServiceResult<bool>`

Generic constraints:
- `TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>`
- All DTOs must inherit from corresponding base DTOs in `Inventorization.Base`

Concrete controllers inherit from `DataController<...>` and require only:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : DataController<Product, CreateProductDTO, UpdateProductDTO, DeleteProductDTO, ProductDetailsDTO, ProductSearchDTO, IProductService>
{
    public ProductsController(IProductService service) : base(service) { }
}
```

### BaseQueryController<TEntity, TProjection>
Abstract base class for complex ADT-based search, filtering, projection, and transformations:
- `POST /api/{entity}/query` → `Query(SearchQuery)` - Returns paginated projection results
- `POST /api/{entity}/query/transform` → `QueryWithTransformations(SearchQuery)` - Returns computed transformation results

Used for complex search scenarios through dedicated `{Entity}sQueryController` classes.

### Controller Design Principles
1. **DRY (Don't Repeat Yourself)**: Eliminate repeated HTTP verb handling and response mapping
2. **Open/Closed Principle**: New controllers extend base classes without modifying existing ones
3. **Template Method Pattern**: Base class defines structure, concrete controllers specify types
4. **Type Safety**: All 7 generic type parameters enforced at compile-time
5. **Minimal Concrete Code**: Concrete controllers should be 3-5 lines (declaration + constructor only)

### HTTP Status Code Mapping
- `200 OK`: `GetByIdAsync`, `UpdateAsync`, `DeleteAsync`, query endpoints success
- `201 Created`: `CreateAsync` success
- `400 Bad Request`: DTO validation failure or domain logic error
- `404 Not Found`: Entity not found during Get/Update/Delete
- `500 Internal Server Error`: Unexpected exceptions

### Adding New Entity Controllers
1. Verify entity has corresponding DTOs in the DTO project (DetailsDTO, CreateDTO, UpdateDTO, DeleteDTO, SearchDTO)
2. Verify `IDataService<...>` interface exists for the entity
3. Register service in `Program.cs` dependency injection
4. Create concrete data controller inheriting from `DataController<...>` and (if needed) a separate `{Entity}sQueryController` inheriting from `BaseQueryController<...>`
5. Define only the route attribute and constructor; inherit all HTTP methods
6. Add controller tests covering inherited methods with mocked service
7. Update Swagger documentation if custom attributes needed

See [CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md) for complete implementation guide, examples, and testing strategies.

---

## Additional Rules & Conventions
- All usages of DTOs, mapping, and projection logic must reference the DTO project and use the `IMapper` abstraction.
- All dependency injection must be by interface, not concrete type.
- All API projects must have a corresponding unit test project covering all concrete abstractions (service, repository, creator, modifier, query provider, etc.).
- Swagger UI must be enabled and served at the root URL in Development mode for all API projects.
- All build, bin, obj, and IDE-specific files must be excluded from version control via `.gitignore`.

**All further backend code, services, and patterns must follow these rules and abstractions.**

# DB
 basically docker-compose file with set of docker images based on official postgresql; each have persistent volumes;

# other
 message broker and MongoDB - also in containers

# DataModel
 described in a separate file DataModel.md