# Controller Architecture Patterns

This document extends Architecture.md with comprehensive guidelines for building generic, reusable controllers using abstract base classes.

## Abstract Generic Controllers Overview

Controllers must be built using a multi-level abstract base class hierarchy to minimize code duplication and enforce consistency:

1. **ServiceController** - Non-generic abstract base (extends `ControllerBase`)
2. **Generic Controllers** - Abstract generic classes (extend `ServiceController`)
   - `DataController<...>` - CRUD operations
3. **Query Controllers** - Dedicated controllers extending `BaseQueryController<TEntity, TProjection>` for complex ADT-based search
4. **Concrete Controllers** - Specific entity controllers (extend generic classes)

All abstract controllers are defined in `Inventorization.[BoundedContextName].API.Base` project. Concrete controllers are in `Inventorization.[BoundedContextName].API` project.

**Benefits:**
- **DRY Principle** - CRUD logic defined once, reused by all controllers
- **Consistency** - All endpoints follow identical request/response patterns
- **Type Safety** - Generic constraints ensure correct DTO types are used
- **Maintainability** - Bug fixes in base classes benefit all derived controllers
- **Scalability** - New controllers added with minimal code
- **Separation of Concerns** - Abstract controllers isolated from entity-specific implementations

---

## ServiceController: Abstract Non-Generic Base Class

**Purpose:** Provide common controller functionality used by all service controllers.

**Location:** `Inventorization.[BoundedContextName].API.Base/Controllers/ServiceController.cs`

**Class Signature:**
```csharp
public abstract class ServiceController : ControllerBase
{
    protected readonly ILogger<ServiceController> Logger;
    
    protected ServiceController(ILogger<ServiceController> logger)
    {
        Logger = logger;
    }
    
    // Common methods:
    // - Response wrapping helpers
    // - Error handling utilities
    // - Validation helpers
    // - Logging utilities
}
```

**Responsibilities:**
- Common response formatting and HTTP status codes
- Centralized error handling and logging
- Request validation patterns
- Cross-cutting concerns

---

## DataController: Abstract Generic CRUD Base Class

**Purpose:** Provide standard Create, Read, Update, Delete operations for all entities.

**Location:** `InventorySystem.API.Base/Controllers/DataController.cs`

**Class Signature:**
```csharp
public abstract class DataController<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO, TService>
    : ServiceController
    where TEntity : class
    where TCreateDTO : class
    where TUpdateDTO : BaseDTO
    where TDeleteDTO : BaseDTO
    where TDetailsDTO : BaseDTO
    where TSearchDTO : class
    where TService : IDataService<TEntity, TCreateDTO, TUpdateDTO, TDeleteDTO, TDetailsDTO, TSearchDTO>
{
    // Inherits logging from ServiceController
}
```

**Generic Type Parameters:**
- `TEntity` - Domain model entity class
- `TCreateDTO` - Data transfer object for create operations
- `TUpdateDTO` - Data transfer object for update operations
- `TDeleteDTO` - Data transfer object for delete operations
- `TDetailsDTO` - Data transfer object for responses (returned to client)
- `TSearchDTO` - Data transfer object for search/filtering operations
- `TService` - Service implementation (must implement full IDataService contract)

**Protected Members:**
```csharp
protected readonly TService _dataService;  // Injected service

protected DataController(TService dataService, ILogger<ServiceController> logger) : base(logger)
{
    _dataService = dataService;
}
```

**Public Virtual Methods to Implement:**

1. **GetByIdAsync(Guid id)** - Retrieve single entity
   - HTTP Method: GET
   - Route: `/{controller}/{id}`
   - Returns: `ActionResult<ServiceResult<TDetailsDTO>>`
   - Success Status: 200 OK
   - Error Statuses: 404 Not Found, 500 Internal Server Error

2. **CreateAsync(TCreateDTO dto)** - Create new entity
   - HTTP Method: POST
   - Route: `/{controller}`
   - Returns: `ActionResult<ServiceResult<TDetailsDTO>>`
   - Success Status: 201 Created
   - Error Statuses: 400 Bad Request, 500 Internal Server Error

3. **UpdateAsync(Guid id, TUpdateDTO dto)** - Update existing entity
   - HTTP Method: PUT
   - Route: `/{controller}/{id}`
   - Validates: URL ID matches DTO.Id
   - Returns: `ActionResult<ServiceResult<TDetailsDTO>>`
   - Success Status: 200 OK
   - Error Statuses: 400 Bad Request, 404 Not Found, 500 Internal Server Error

4. **DeleteAsync(Guid id)** - Delete entity
   - HTTP Method: DELETE
   - Route: `/{controller}/{id}`
   - Returns: `ActionResult<ServiceResult<bool>>`
   - Success Status: 200 OK
   - Error Statuses: 404 Not Found, 500 Internal Server Error

**Base Class Responsibilities:**
- HTTP verb and route mapping
- Service method delegation
- Response wrapping in ServiceResult<T>
- HTTP status code determination
- Error handling and logging
- Request validation

## BaseQueryController: Query/Search Base Class

**Purpose:** Provide ADT-based querying with filtering, projection, sorting, pagination, and transformations.

**Location:** `InventorySystem.API.Base/Controllers/BaseQueryController.cs`

**Class Signature:**
```csharp
public abstract class BaseQueryController<TEntity, TProjection> : ControllerBase
    where TEntity : class
    where TProjection : class, new()
```

**Public Virtual Methods:**

1. **Query(SearchQuery query)**
   - HTTP Method: POST
   - Route: `/api/{entity}/query`
   - Returns: `ActionResult<ServiceResult<SearchResult<TProjection>>>`

2. **QueryWithTransformations(SearchQuery query)**
   - HTTP Method: POST
   - Route: `/api/{entity}/query/transform`
   - Returns: `ActionResult<ServiceResult<SearchResult<TransformationResult>>>`

## Future Controller Base Classes (Roadmap)

**To be implemented in future phases:**
- `ReportController<T>` - Complex reporting and analytics queries
- `AuthorizedController<T>` - Authorization and permission checks
- `CachedController<T>` - Caching strategies and cache invalidation
- `EventSourcedController<T>` - Event sourcing pattern support
- `GraphQLController<T>` - GraphQL endpoint support

## Concrete Controller Implementation

**Concrete controllers should only contain:**
1. Class declaration with inheritance from abstract controller(s)
2. Route and API controller attributes
3. Constructor injecting specific service implementation
4. Entity-specific customizations (optional, rare)

### Folder Structure & Location

All concrete controllers are located in `Inventorization.[BoundedContextName].API` project, organized by base controller type:

```
Inventorization.[BoundedContextName].API/
├── Controllers/
│   ├── ProductsController.cs              (extends DataController<...>)
│   ├── CategoriesController.cs            (extends DataController<...>)
│   ├── ProductsQueryController.cs         (extends BaseQueryController<...>)
│   ├── CategoriesQueryController.cs       (extends BaseQueryController<...>)
│   └── ...
```

**Naming Convention:**
- `[Entity]sController.cs` for CRUD operations via `DataController<...>`
- `[Entity]sQueryController.cs` for complex ADT query/search via `BaseQueryController<...>`
- `[Entity][ControllerType]Controller.cs` for specialized controllers

**Minimal DataController Implementation:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : DataController<
    Product,                    // TEntity
    CreateProductDTO,           // TCreateDTO
    UpdateProductDTO,           // TUpdateDTO
    DeleteProductDTO,           // TDeleteDTO
    ProductDetailsDTO,          // TDetailsDTO
    ProductSearchDTO,           // TSearchDTO
    IProductService>            // TService
{
    public ProductsController(
        IProductService dataService,
        ILogger<ProductsController> logger)
        : base(dataService, logger)
    {
    }
    
    // All CRUD methods automatically inherited!
    // No additional implementation needed.
}
```

**With Query Capabilities (separate controller):**
```csharp
[ApiController]
[Route("api/products/query")]
public class ProductsQueryController : BaseQueryController<Product, ProductProjection>
{
    public ProductsQueryController(
        ISearchService<Product, ProductProjection> searchService,
        ILogger<ProductsQueryController> logger)
        : base(searchService, logger)
    {
    }
}
```

## Standard Endpoint Routes

**DataController Endpoints:**
```
GET    /api/{entity}/{id}              - Get by ID
POST   /api/{entity}                   - Create new entity
PUT    /api/{entity}/{id}              - Update entity
DELETE /api/{entity}/{id}              - Delete entity
```

**BaseQueryController Endpoints:**
```
POST   /api/{entity}/query             - ADT query with filters/projection/sort/pagination
POST   /api/{entity}/query/transform   - Query with computed field transformations
```

## Response Format

**All responses wrapped in consistent ServiceResult<T> format:**

**Success Response (Status 200, 201):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid-value",
    "name": "Entity Name",
    "createdAt": "2026-01-31T10:00:00Z"
  },
  "message": "Operation completed successfully",
  "errors": []
}
```

**Validation Error Response (Status 400):**
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    "Field 'Name' is required",
    "Field 'Price' must be greater than 0"
  ]
}
```

**Not Found Response (Status 404):**
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Product not found",
  "errors": []
}
```

**Server Error Response (Status 500):**
```json
{
  "isSuccess": false,
  "data": null,
  "message": "An error occurred while processing your request",
  "errors": ["Internal error details"]
}
```

## HTTP Status Code Mapping

| Operation | Success | Validation Error | Not Found | Server Error |
|-----------|---------|------------------|-----------|-----------------|
| GetById   | 200     | -                | 404       | 500             |
| Create    | 201     | 400              | -         | 500             |
| Update    | 200     | 400              | 404       | 500             |
| Delete    | 200     | -                | 404       | 500             |
| Query     | 200     | 400              | -         | 500             |

## Adding New Entity Controllers

**Complete checklist to add a new entity's controller:**

1. **Create DTOs** in `InventorySystem.DTOs/DTO/{EntityName}/`:
   - `Create{Entity}DTO` (extends CreateDTO)
   - `Update{Entity}DTO` (extends UpdateDTO)
   - `Delete{Entity}DTO` (extends DeleteDTO)
   - `{Entity}DetailsDTO` (extends DetailsDTO)
   - `{Entity}SearchDTO` (extends SearchDTO)

2. **Create Service Interface** `I{Entity}Service.cs`:
   ```csharp
   public interface I{Entity}Service : IDataService<
       {Entity},
       Create{Entity}DTO,
       Update{Entity}DTO,
       Delete{Entity}DTO,
       {Entity}DetailsDTO,
       {Entity}SearchDTO>
   {
   }
   ```

3. **Implement Required Abstractions** in Business layer:
   - Mapper: `IMapper<{Entity}, {Entity}DetailsDTO>`
   - Creator: `IEntityCreator<{Entity}, Create{Entity}DTO>`
   - Modifier: `IEntityModifier<{Entity}, Update{Entity}DTO>`
   - Search Provider: `ISearchQueryProvider<{Entity}, {Entity}SearchDTO>`

4. **Implement Data Service** `{Entity}DataService.cs` implementing `I{Entity}Service`

5. **Register in DI Container** in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<I{Entity}Service, {Entity}DataService>();
   ```

6. **Create Concrete Controller** `{Entity}sController.cs`:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class {Entity}sController : DataController<
       {Entity}, Create{Entity}DTO, Update{Entity}DTO,
       Delete{Entity}DTO, {Entity}DetailsDTO,
       {Entity}SearchDTO, I{Entity}Service>
   {
       public {Entity}sController(
           I{Entity}Service dataService,
           ILogger<{Entity}sController> logger)
           : base(dataService, logger)
       {
       }
   }
   ```

7. **Result:** Full CRUD API with no additional controller code needed!

## Controller Inheritance Chain

```
Microsoft.AspNetCore.Mvc.ControllerBase
    ↓
Inventorization.Base.Controllers.DataController<...>
    ↓
ProductsController (IProductService injected)
CategoriesController (ICategoryService injected)
StockController (IStockMovementService injected)
[All future entity controllers]
```

## Design Principles Applied

1. **DRY (Don't Repeat Yourself)** - CRUD logic defined once in base class
2. **SOLID - Single Responsibility** - Each class has one reason to change
3. **SOLID - Open/Closed** - Open for extension (new controllers), closed for modification
4. **SOLID - Liskov Substitution** - All concrete controllers satisfy DataController contract
5. **Generic Type Parameters** - Enable compile-time type safety and reusability
6. **Template Method Pattern** - Base class defines algorithm structure, concrete classes inherit
7. **Dependency Injection** - All dependencies injected via constructor

## Testing Abstract Controllers

- **Unit Tests for DataController<T>** - Test generic CRUD behavior once (HTTP mapping, response wrapping, error handling)
- **Integration Tests for Concrete Controllers** - Test specific entity service integration
- **Mock TService** in DataController unit tests to verify request/response handling
- **Use Real Services** in integration tests for end-to-end validation
- **Test Base Class Once** - All derived controllers inherit tested behavior

## Integration with Existing Architecture

This controller pattern integrates seamlessly with the existing architecture:

1. **Services Layer** - Controllers delegate to `IDataService<T>` implementations (ProductDataService, CategoryDataService, StockMovementDataService)
2. **DTOs Layer** - Concrete generic parameters specify exact DTO types for each entity
3. **Dependency Injection** - Services registered in DI container are injected into controller constructor
4. **Response Wrapping** - All results wrapped in `ServiceResult<T>` for consistency
5. **Audit Logging** - Services automatically log via `IAuditLogger` (no controller changes needed)
6. **Error Handling** - Unified approach across all controllers via base class implementation

---

## Relationship Management Controllers

Controllers managing entity relationships implement `IRelationController<TRelatedEntity>` for each relationship type. This pattern provides RESTful endpoints for adding/removing entity associations.

### IRelationController<TRelatedEntity> Interface

Located in `InventorySystem.API.Base/Controllers/IRelationController.cs`:

```csharp
public interface IRelationController<TRelatedEntity>
    where TRelatedEntity : class
{
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> UpdateRelationshipsAsync(
        Guid id,
        EntityReferencesDTO changes,
        CancellationToken cancellationToken = default);
    
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> UpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken = default);
}
```

### DataRelationHandler<TEntity, TRelatedEntity> Abstract Base Class

Located in `InventorySystem.API.Base/Controllers/DataRelationHandler.cs`:

Provides boilerplate implementation for relationship update operations:

```csharp
public abstract class DataRelationHandler<TEntity, TRelatedEntity>
    where TEntity : class
    where TRelatedEntity : class
{
    protected readonly IRelationshipManager<TEntity, TRelatedEntity> RelationshipManager;
    protected readonly ILogger Logger;
    
    protected async Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> HandleUpdateRelationshipsAsync(
        Guid id,
        EntityReferencesDTO changes,
        string relationshipName,
        CancellationToken cancellationToken);
    
    protected async Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> HandleUpdateMultipleRelationshipsAsync(
        Dictionary<Guid, EntityReferencesDTO> changes,
        string relationshipName,
        CancellationToken cancellationToken);
}
```

**Features:**
- Consistent logging and error handling
- Validation of input parameters
- HTTP status code mapping (200 OK, 400 Bad Request, 500 Internal Server Error)
- Integration with `IRelationshipManager<TEntity, TRelatedEntity>`

### Route Conventions

**Single Entity Relationship Update:**
```
PATCH /api/{entity}/{id}/relationships/{relationName}
```

**Bulk Relationship Update:**
```
PATCH /api/{entity}/relationships/{relationName}/bulk
```

### Complete Controller Example

Controller managing multiple relationships (User ↔ Role, User ↔ Team):

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : 
    DataController<User, CreateUserDTO, UpdateUserDTO, DeleteUserDTO, UserDetailsDTO, UserSearchDTO, IUserDataService>,
    IRelationController<Role>,  // User-Role relationships
    IRelationController<Team>   // User-Team relationships
{
    // Relationship handlers (nested private classes)
    private readonly UserRoleRelationHandler _roleHandler;
    private readonly UserTeamRelationHandler _teamHandler;

    public UsersController(
        IUserDataService dataService,
        IRelationshipManager<User, Role> roleRelationshipManager,
        IRelationshipManager<User, Team> teamRelationshipManager,
        ILogger<UsersController> logger)
        : base(dataService, logger)
    {
        _roleHandler = new UserRoleRelationHandler(roleRelationshipManager, logger);
        _teamHandler = new UserTeamRelationHandler(teamRelationshipManager, logger);
    }

    // IRelationController<Role> implementation
    /// <summary>
    /// Updates role assignments for this user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="changes">Roles to add/remove</param>
    [HttpPatch("{id}/relationships/roles")]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 200)]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 400)]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 500)]
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Role>.UpdateRelationshipsAsync(
        Guid id,
        [FromBody] EntityReferencesDTO changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateRelationshipsAsync(id, changes, "Roles", cancellationToken);
    }

    /// <summary>
    /// Bulk update role assignments for multiple users
    /// </summary>
    [HttpPatch("relationships/roles/bulk")]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 200)]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 400)]
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> IRelationController<Role>.UpdateMultipleRelationshipsAsync(
        [FromBody] Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken)
    {
        return _roleHandler.HandleUpdateMultipleRelationshipsAsync(changes, "Roles", cancellationToken);
    }

    // IRelationController<Team> implementation
    /// <summary>
    /// Updates team memberships for this user
    /// </summary>
    [HttpPatch("{id}/relationships/teams")]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 200)]
    [ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 400)]
    Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Team>.UpdateRelationshipsAsync(
        Guid id,
        [FromBody] EntityReferencesDTO changes,
        CancellationToken cancellationToken)
    {
        return _teamHandler.HandleUpdateRelationshipsAsync(id, changes, "Teams", cancellationToken);
    }

    /// <summary>
    /// Bulk update team memberships for multiple users
    /// </summary>
    [HttpPatch("relationships/teams/bulk")]
    [ProducesResponseType(typeof(ServiceResult<BulkRelationshipUpdateResult>), 200)]
    Task<ActionResult<ServiceResult<BulkRelationshipUpdateResult>>> IRelationController<Team>.UpdateMultipleRelationshipsAsync(
        [FromBody] Dictionary<Guid, EntityReferencesDTO> changes,
        CancellationToken cancellationToken)
    {
        return _teamHandler.HandleUpdateMultipleRelationshipsAsync(changes, "Teams", cancellationToken);
    }

    // Nested handler classes (private to controller)
    private class UserRoleRelationHandler : DataRelationHandler<User, Role>
    {
        public UserRoleRelationHandler(IRelationshipManager<User, Role> manager, ILogger logger)
            : base(manager, logger) { }
    }

    private class UserTeamRelationHandler : DataRelationHandler<User, Team>
    {
        public UserTeamRelationHandler(IRelationshipManager<User, Team> manager, ILogger logger)
            : base(manager, logger) { }
    }
}
```

### Request/Response Formats

**Single Update Request:**
```http
PATCH /api/users/{{userId}}/relationships/roles
Content-Type: application/json

{
  "idsToAdd": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "7c9e6679-7425-40de-944b-e07fc1f90ae7"
  ],
  "idsToRemove": [
    "9b2def3c-3b13-4a3d-a21e-af5c0f0b4c8a"
  ]
}
```

**Single Update Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "isSuccess": true,
    "addedCount": 2,
    "removedCount": 1,
    "message": "Added 2, removed 1 relationships",
    "errors": []
  },
  "message": "Updated Roles relationships: added 2, removed 1",
  "errors": []
}
```

**Bulk Update Request:**
```http
PATCH /api/users/relationships/roles/bulk
Content-Type: application/json

{
  "3fa85f64-5717-4562-b3fc-2c963f66afa6": {
    "idsToAdd": ["role-id-1", "role-id-2"],
    "idsToRemove": []
  },
  "7c9e6679-7425-40de-944b-e07fc1f90ae7": {
    "idsToAdd": [],
    "idsToRemove": ["role-id-3"]
  }
}
```

**Bulk Update Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "isSuccess": true,
    "totalAdded": 2,
    "totalRemoved": 1,
    "successfulOperations": 2,
    "failedOperations": 0,
    "message": "Bulk operation completed: 2 successful, added 2, removed 1",
    "errors": [],
    "operationResults": {
      "3fa85f64-5717-4562-b3fc-2c963f66afa6": {
        "isSuccess": true,
        "addedCount": 2,
        "removedCount": 0
      },
      "7c9e6679-7425-40de-944b-e07fc1f90ae7": {
        "isSuccess": true,
        "addedCount": 0,
        "removedCount": 1
      }
    }
  },
  "message": "Bulk update completed: 2 successful, added 2, removed 1",
  "errors": []
}
```

**Error Response (400 Bad Request):**
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    "Role 9b2def3c-3b13-4a3d-a21e-af5c0f0b4c8a not found",
    "Cannot assign more than 10 roles at once"
  ]
}
```

### OpenAPI/Swagger Attributes

Use `ProducesResponseType` for proper Swagger documentation:

```csharp
[HttpPatch("{id}/relationships/roles")]
[ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 200)]
[ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 400)]
[ProducesResponseType(typeof(ServiceResult<RelationshipUpdateResult>), 500)]
[SwaggerOperation(
    Summary = "Update role assignments",
    Description = "Adds and/or removes role assignments for the specified user",
    OperationId = "UpdateUserRoles",
    Tags = new[] { "Users", "Relationships" }
)]
```

### Explicit Interface Implementation

When a controller implements multiple `IRelationController<T>` interfaces, use explicit implementation to avoid ambiguity:

```csharp
// CORRECT: Explicit interface implementation
Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Role>.UpdateRelationshipsAsync(...)
{
    return _roleHandler.HandleUpdateRelationshipsAsync(...);
}

Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> IRelationController<Team>.UpdateRelationshipsAsync(...)
{
    return _teamHandler.HandleUpdateRelationshipsAsync(...);
}

// INCORRECT: Would cause compilation error
public Task<ActionResult<ServiceResult<RelationshipUpdateResult>>> UpdateRelationshipsAsync(...)
{
    // Ambiguous - which relationship?
}
```

### Nested Handler Pattern

Organizing relationship handlers as nested private classes keeps related code together:

```csharp
public class UsersController : DataController<...>, IRelationController<Role>
{
    private readonly UserRoleRelationHandler _roleHandler;
    
    public UsersController(...)
    {
        _roleHandler = new UserRoleRelationHandler(roleRelationshipManager, logger);
    }
    
    // Nested class has access to controller context if needed
    private class UserRoleRelationHandler : DataRelationHandler<User, Role>
    {
        public UserRoleRelationHandler(IRelationshipManager<User, Role> manager, ILogger logger)
            : base(manager, logger) { }
    }
}
```

### Dependency Injection Registration

Register relationship managers in `Program.cs`:

```csharp
// Relationship managers
builder.Services.AddScoped<IRelationshipManager<User, Role>, UserRoleRelationshipManager>();
builder.Services.AddScoped<IRelationshipManager<User, Team>, UserTeamRelationshipManager>();

// Validators
builder.Services.AddScoped<IValidator<EntityReferencesDTO>, EntityReferencesValidator>();
```

### Design Principles

1. **Single Responsibility**: Each handler manages one relationship type
2. **Open/Closed**: New relationships added by implementing additional `IRelationController<T>`
3. **Explicit over Implicit**: Relationship names in routes prevent ambiguity
4. **Type Safety**: Generic constraints ensure correct entity types
5. **Consistency**: All relationship operations follow identical patterns

### Testing Relationship Controllers

**Unit Tests:**
- Mock `IRelationshipManager<TEntity, TRelatedEntity>`
- Verify HTTP status codes for success/failure scenarios
- Test request validation (empty changes, invalid IDs)
- Verify response wrapping in `ServiceResult<T>`

**Integration Tests:**
- Test actual relationship creation/deletion in database
- Verify transaction rollback on errors
- Test bulk operations with mixed success/failure
- Validate cascade delete prevention

### Adding New Relationship Controllers

**Checklist:**
1. ✅ Implement `IRelationshipManager<TParent, TRelated>` in Domain layer
2. ✅ Implement `IValidator<EntityReferencesDTO>` for relationship validation
3. ✅ Register both in DI container
4. ✅ Add `IRelationController<TRelated>` to parent controller
5. ✅ Create nested handler class extending `DataRelationHandler<TParent, TRelated>`
6. ✅ Implement interface methods with `[HttpPatch]` attributes and proper routes
7. ✅ Add OpenAPI documentation attributes
8. ✅ Test relationship endpoints

See [Architecture.md](Architecture.md) "Entity Relationship Management Patterns" section for complete guidance on when to use relationship controllers vs full CRUD junction entities.

