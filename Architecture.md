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
- All base abstractions and common data structures (such as `CreateDTO`, `UpdateDTO`, `DeleteDTO`, `DetailsDTO`, `SearchDTO`, `PageDTO`, `ServiceResult<T>`, `UnitOfWorkBase<TDbContext>`, and all generic interfaces like `IEntityCreator`, `IEntityModifier`, `ISearchQueryProvider`, `IMapper`, `IUnitOfWork`, etc.) must be located in a separate shared project named `Inventorization.Base`.
- All bounded context/domain projects must reference `Inventorization.Base` for these shared types.
- All DTOs for each bounded context must be placed in the corresponding DTO project under a `DTO/` subfolder (e.g., `DTO/Customer`).
- All mapping logic (entity-to-DTO and DTO-to-entity) must use the `IMapper<TEntity, TDetailsDTO>` abstraction, supporting both object mapping and LINQ projection via `Expression<Func<TEntity, TDetailsDTO>>`.

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