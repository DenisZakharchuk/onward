# Goods Bounded Context

## Overview
The Goods bounded context manages products/items in the inventory system. It follows the microservices architecture pattern with complete CRUD operations and JWT-based authentication.

## Project Structure

### 1. Inventorization.Goods.DTO
**Type**: Class Library  
**Purpose**: Data Transfer Objects for the Goods context  
**Key Components**:
- `DTO/Good/CreateGoodDTO` - Creating new goods
- `DTO/Good/UpdateGoodDTO` - Updating existing goods
- `DTO/Good/DeleteGoodDTO` - Deleting goods
- `DTO/Good/GoodDetailsDTO` - Response DTO for good details
- `DTO/Good/GoodSearchDTO` - Search/filter criteria

### 2. Inventorization.Goods.Domain
**Type**: Class Library  
**Purpose**: Domain logic, entities, and data access  
**Key Components**:
- **Entities**: `Good` entity with immutability pattern
- **DbContexts**: `GoodsDbContext` with EF Core configuration
- **DataAccess**: `GoodsUnitOfWork` implementing transaction support
- **Creators**: `GoodCreator` - Creates entities from DTOs
- **Modifiers**: `GoodModifier` - Updates entities from DTOs
- **Mappers**: `GoodMapper` - Maps entities to DTOs with LINQ projection support
- **SearchProviders**: `GoodSearchProvider` - Generates search expressions
- **Validators**: `CreateGoodValidator`, `UpdateGoodValidator`
- **DataServices**: `GoodDataService` inheriting from `DataServiceBase`

### 3. Inventorization.Goods.API
**Type**: ASP.NET Web App  
**Purpose**: REST API for Goods operations  
**Port**: 5013  
**Key Features**:
- JWT Bearer authentication
- Swagger UI (Development mode at root `/`)
- Generic `GoodsController` inheriting all CRUD operations from `DataController`

### 4. Inventorization.Goods.API.Tests
**Type**: xUnit Test Project  
**Purpose**: Unit tests for all abstractions  
**Status**: ✅ 1 sample test passing

## Entity: Good

### Properties
- `Id` (Guid) - Primary key
- `Name` (string, required, max 200) - Good name
- `Description` (string?, max 1000) - Optional description
- `Sku` (string, required, max 50, unique) - Stock keeping unit
- `UnitPrice` (decimal) - Price per unit
- `QuantityInStock` (int) - Current stock quantity
- `UnitOfMeasure` (string?, max 50) - Unit of measurement
- `IsActive` (bool) - Active status flag
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime?) - Last update timestamp

### Immutability Pattern
The `Good` entity follows strict immutability:
- Private setters on all properties
- Private parameterless constructor (EF Core only)
- Public parameterized constructor with validation
- State mutations through dedicated methods:
  - `Update()` - Updates basic information
  - `UpdateQuantity()` - Updates stock quantity
  - `UpdatePrice()` - Updates unit price
  - `Activate()` / `Deactivate()` - Manages active status

## API Endpoints

All endpoints require JWT Bearer authentication.

### GET /api/goods/{id}
Retrieve a single good by ID

### POST /api/goods
Create a new good  
**Body**: `CreateGoodDTO`

### PUT /api/goods/{id}
Update an existing good  
**Body**: `UpdateGoodDTO`

### DELETE /api/goods/{id}
Delete a good (soft delete via deactivation)  
**Body**: `DeleteGoodDTO`

### POST /api/goods/search
Search/filter goods with pagination  
**Body**: `GoodSearchDTO`

## Database Configuration

### Connection String
```json
"ConnectionStrings": {
  "GoodsDatabase": "Host=localhost;Port=5432;Database=inventorization_goods;Username=postgres;Password=postgres"
}
```

### Schema
- **Table**: `Goods`
- **Indexes**:
  - Primary key on `Id`
  - Unique index on `Sku`
  - Index on `IsActive`
  - Index on `CreatedAt`

## Running the API

### Prerequisites
- .NET 8 SDK
- PostgreSQL database (containerized or local)

### Build
```bash
cd backend
dotnet build Inventorization.Goods.API/Inventorization.Goods.API.csproj
```

### Run
```bash
cd backend/Inventorization.Goods.API
dotnet run
```

API will be available at: http://localhost:5013  
Swagger UI: http://localhost:5013 (Development mode)

### Run Tests
```bash
cd backend
dotnet test Inventorization.Goods.API.Tests/Inventorization.Goods.API.Tests.csproj
```

## Architecture Compliance

✅ **Entity Immutability**: All properties have private setters, state changes via methods  
✅ **Dependency Injection**: All dependencies injected as interfaces  
✅ **DataServiceBase**: Uses generic `DataServiceBase` from `Inventorization.Base`  
✅ **DTO Inheritance**: All DTOs inherit from base DTOs in `Inventorization.Base`  
✅ **Abstraction Pattern**: Complete implementations of all required interfaces:
- `IMapper<Good, GoodDetailsDTO>`
- `IEntityCreator<Good, CreateGoodDTO>`
- `IEntityModifier<Good, UpdateGoodDTO>`
- `ISearchQueryProvider<Good, GoodSearchDTO>`
- `IValidator<CreateGoodDTO>`
- `IValidator<UpdateGoodDTO>`

✅ **Controller Base**: `GoodsController` extends generic `DataController`  
✅ **Unit Tests**: Test project scaffolded and passing  
✅ **PostgreSQL**: Dedicated database per microservice principle

## Next Steps

1. **Database Migration**: Run EF Core migrations to create database schema
   ```bash
   dotnet ef migrations add InitialCreate --project Inventorization.Goods.Domain --startup-project Inventorization.Goods.API
   dotnet ef database update --project Inventorization.Goods.Domain --startup-project Inventorization.Goods.API
   ```

2. **Add Unit Tests**: Implement tests for:
   - `GoodCreator`
   - `GoodModifier`
   - `GoodMapper`
   - `GoodSearchProvider`
   - `CreateGoodValidator`
   - `UpdateGoodValidator`
   - `GoodDataService`

3. **Container Configuration**: Add to docker-compose.yml for containerized deployment

4. **Frontend Integration**: Create Vue.js components for Goods management UI

## Dependencies

- `Inventorization.Base` - Shared abstractions and base types
- `InventorySystem.API.Base` - Shared API base controllers
- Entity Framework Core 8.0
- Npgsql for PostgreSQL
- JWT Bearer Authentication
- Swagger/OpenAPI

## JWT Configuration

```json
"JwtSettings": {
  "SecretKey": "your-super-secret-key-change-in-production-min-32-chars",
  "Issuer": "Inventorization.Auth",
  "Audience": "Inventorization.Client",
  "ExpirationMinutes": 60
}
```

**⚠️ Important**: Change the `SecretKey` in production environment!
