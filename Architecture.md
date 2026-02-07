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
 - Inventorization.[BoundedContextName].Domain; class library; It has folders: Entities, Services, DbContexts, UOWs. Inventorization.[BoundedContextName].DTO is a dependency;
 - Inventorization.[BoundedContextName].API; asp.net web app; Inventorization.[BoundedContextName].Domain and Inventorization.[BoundedContextName].DTO are dependencies
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

## Dependency Injection Rules
- All DataService implementations (e.g., CustomerService) must be injected as their corresponding IDataService<T...> or specific interface (e.g., ICustomerService), not as concrete types.
- All service, repository, and abstraction dependencies should be injected as interfaces, not concrete types, to maximize testability and flexibility.

## Testing Rules
- Every API project must have a corresponding unittest project (e.g., Inventorization.[BoundedContextName].API.Tests).
- Every concrete abstraction (service, repository, creator, modifier, query provider, etc.) must be covered with unit tests in the unittest project.

## General Principles
  - DTO projects: `Inventorization.[BoundedContextName].DTO` (class library)
  - Domain projects: `Inventorization.[BoundedContextName].Domain` (class library, with Entities, Services, DbContexts, UOWs)
  - API projects: `Inventorization.[BoundedContextName].API` (ASP.NET web app)

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
// In BoundedContext.Domain/DataAccess/
public interface IGoodsUnitOfWork : Inventorization.Base.DataAccess.IUnitOfWork
{
    // Add bounded context-specific methods if needed (usually empty)
}
```

**Step 2: Implement concrete UnitOfWork** (inherits from `UnitOfWorkBase<TDbContext>`):

```csharp
// In BoundedContext.Domain/DataAccess/
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

## Domain Service Abstractions

All bounded contexts should use the **generic `DataServiceBase<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>`** class located in `Inventorization.Base.Services` for implementing data services. This eliminates boilerplate CRUD code while enforcing consistent patterns across all contexts.

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

**Step-by-step example** for a `Customer` entity:

```csharp
// ICustomerDataService.cs
public interface ICustomerDataService : IDataService<Customer, CreateCustomerDTO, UpdateCustomerDTO, DeleteCustomerDTO, CustomerDetailsDTO, CustomerSearchDTO>
{
}

// CustomerDataService.cs
using Inventorization.Base.Services;
using Inventorization.Base.DataAccess;
using Inventorization.Base.Abstractions;
using Microsoft.Extensions.Logging;

public class CustomerDataService : DataServiceBase<Customer, CreateCustomerDTO, UpdateCustomerDTO, DeleteCustomerDTO, CustomerDetailsDTO, CustomerSearchDTO>, ICustomerDataService
{
    public CustomerDataService(
        Inventorization.Base.DataAccess.IUnitOfWork unitOfWork,
        IRepository<Customer> repository,
        IServiceProvider serviceProvider,
        ILogger<DataServiceBase<Customer, CreateCustomerDTO, UpdateCustomerDTO, DeleteCustomerDTO, CustomerDetailsDTO, CustomerSearchDTO>> logger)
        : base(unitOfWork, repository, serviceProvider, logger)
    {
    }
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

`DataServiceBase` uses lazy dependency resolution to minimize overhead:

**Constructor (always injected):**
```csharp
IUnitOfWorkInterface unitOfWork,     // For SaveChangesAsync
IRepository<TEntity> repository,     // For CRUD operations
IServiceProvider serviceProvider,    // For lazy resolution
ILogger<...> logger                  // For logging
```

**Runtime resolution (resolved only when needed):**
- `IMapper<TEntity, TDetailsDTO>` - Resolved in GetByIdAsync, AddAsync, UpdateAsync, SearchAsync
- `IValidator<TCreateDTO>` - Resolved in AddAsync
- `IValidator<TUpdateDTO>` - Resolved in UpdateAsync
- `IEntityCreator<TEntity, TCreateDTO>` - Resolved in AddAsync
- `IEntityModifier<TEntity, TUpdateDTO>` - Resolved in UpdateAsync
- `ISearchQueryProvider<TEntity, TSearchDTO>` - Resolved in SearchAsync

This approach:
- Reduces constructor complexity
- Minimizes DI container pressure
- Only instantiates dependencies when actually used
- Allows each bounded context to register only what it needs

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

#### 1. Junction Entity (in BoundedContext.Domain/Entities)

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

#### 2. Property Accessors (in BoundedContext.Domain/PropertyAccessors)

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

#### 3. Concrete Relationship Manager (in BoundedContext.Domain/DataServices)

**Step-by-step example** for a `User ↔ Role` relationship:

```csharp
using Inventorization.Base.DataAccess;
using Inventorization.Base.Services;
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
        ILogger<UserRoleRelationshipManager> logger)
        : base(userRepository, roleRepository, userRoleRepository, unitOfWork, serviceProvider, logger)
    {
    }
    
    // That's it! Property accessors are resolved from DI automatically.
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

// 3. Register relationship manager
builder.Services.AddScoped<IRelationshipManager<User, Role>, UserRoleRelationshipManager>();

// 4. Register validator
builder.Services.AddScoped<IValidator<EntityReferencesDTO>, EntityReferencesValidator>();
```

**Important:** Property accessors must be registered for every junction entity type. This is a **required** part of the bounded context boilerplate.

### Benefits

- **88%+ code reduction**: ~24 lines instead of ~200 lines
- **Dependency injection**: Property accessors resolved via DI, following framework conventions
- **Consistency**: All relationship managers follow identical patterns
- **Type safety**: Expression-based approach provides compile-time validation
- **Performance**: Expressions compile to efficient EF Core SQL queries, getters are cached
- **Maintainability**: Business logic changes only need to be made once in base class
- **Testability**: Mock/stub property accessors and relationship managers independently
- **Reusability**: Property accessors can be used by mappers, validators, and other components

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
  - `Controllers/SearchController.cs` - Generic search base extending `ServiceController`
- **Inventorization.[BoundedContextName].API** - Concrete controllers
  - References `...API.Base` project
  - All controllers inherit from generic base classes
  - Folder structure mirrors base controller types:
    - `Controllers/Data/` - Concrete controllers extending `DataController<...>`
    - `Controllers/Search/` - Concrete controllers extending `SearchController<...>`
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
- `DELETE /{id}` → `DeleteAsync(id, dto)` - Returns `ServiceResult<TDetailsDTO>`

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

### SearchController<TEntity, TDetailsDTO, TSearchDTO, TService>
Abstract base class for complex search, filtering, and pagination:
- `POST /search` → `SearchAsync(dto)` - Returns paginated results with `ServiceResult<PageDTO<TDetailsDTO>>`
- Supports custom filters, projections, and sorting via `SearchDTO`

Used when search complexity exceeds simple `GetByIdAsync` queries. Inherited alongside or instead of `DataController` depending on entity requirements.

### Controller Design Principles
1. **DRY (Don't Repeat Yourself)**: Eliminate repeated HTTP verb handling and response mapping
2. **Open/Closed Principle**: New controllers extend base classes without modifying existing ones
3. **Template Method Pattern**: Base class defines structure, concrete controllers specify types
4. **Type Safety**: All 7 generic type parameters enforced at compile-time
5. **Minimal Concrete Code**: Concrete controllers should be 3-5 lines (declaration + constructor only)

### HTTP Status Code Mapping
- `200 OK`: `GetByIdAsync`, `UpdateAsync`, `SearchAsync` success
- `201 Created`: `CreateAsync` success
- `400 Bad Request`: DTO validation failure or domain logic error
- `404 Not Found`: Entity not found during Get/Update/Delete
- `500 Internal Server Error`: Unexpected exceptions

### Adding New Entity Controllers
1. Verify entity has corresponding DTOs in the DTO project (DetailsDTO, CreateDTO, UpdateDTO, DeleteDTO, SearchDTO)
2. Verify `IDataService<...>` interface exists for the entity
3. Register service in `Program.cs` dependency injection
4. Create concrete controller class inheriting from `DataController<...>` or `SearchController<...>`
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