# Project Structure Overview

## Backend Projects

### InventorySystem.API (Presentation Layer)
**Purpose**: Web API host and HTTP request handling

**Key Files**:
- `Program.cs` - Application entry point, DI configuration, CORS setup
- `Controllers/ProductsController.cs` - Product CRUD endpoints
- `Controllers/CategoriesController.cs` - Category CRUD endpoints
- `Controllers/StockController.cs` - Stock movement endpoints

### InventorySystem.Business (Business Logic Layer)
**Purpose**: Business logic and orchestration

**Key Files**:
- `Services/ProductService.cs` - Product business logic
- `Services/CategoryService.cs` - Category business logic
- `Services/StockService.cs` - Stock movement logic with transactions

### InventorySystem.DTOs (Data Transfer Objects)
**Purpose**: Shared data contracts between layers

**Key Files**:
- `ProductDto.cs`, `CreateProductDto.cs`, `UpdateProductDto.cs`
- `CategoryDto.cs`, `CreateCategoryDto.cs`
- `StockMovementDto.cs`, `CreateStockMovementDto.cs`

### InventorySystem.DataAccess (Data Access Layer)
**Purpose**: Data models and repository abstractions

**Key Directories**:
- `Models/` - Domain entities (Product, Category, StockMovement)
- `Abstractions/` - Repository interfaces
- `Repositories/` - In-memory implementations (placeholder)

**Key Files**:
- `Abstractions/IRepository.cs` - Generic repository interface
- `Abstractions/IProductRepository.cs` - Product-specific operations
- `Abstractions/ICategoryRepository.cs` - Category-specific operations
- `Abstractions/IStockMovementRepository.cs` - Stock movement operations
- `Abstractions/IUnitOfWork.cs` - Transaction management
- `Repositories/InMemoryProductRepository.cs` - Placeholder implementation
- `Repositories/InMemoryCategoryRepository.cs` - Placeholder implementation
- `Repositories/InMemoryStockMovementRepository.cs` - Placeholder implementation
- `Repositories/InMemoryUnitOfWork.cs` - Placeholder implementation

## Frontend Structure

### src/
**Main Application**:
- `main.ts` - Application entry point
- `App.vue` - Root component with navigation
- `router/index.ts` - Vue Router configuration

### src/components/
**Vue Components**:
- `ProductsView.vue` - Product management UI
- `CategoriesView.vue` - Category management UI
- `StockView.vue` - Stock movement tracking UI

### src/services/
**API Communication Layer**:
- `api.ts` - Axios configuration and base URL
- `productService.ts` - Product API calls
- `categoryService.ts` - Category API calls
- `stockService.ts` - Stock movement API calls

### src/types/
**TypeScript Definitions**:
- `index.ts` - All TypeScript interfaces and types

## Project Dependencies

### Backend (.NET 8)
All projects target .NET 8.0:
- InventorySystem.API
  - References: Business, DTOs
  - ASP.NET Core Web API
- InventorySystem.Business
  - References: DataAccess, DTOs
- InventorySystem.DataAccess
  - References: DTOs
- InventorySystem.DTOs
  - No dependencies (shared library)

### Frontend (Vue.js + Vite)
- Vue 3
- Vue Router
- Axios
- TypeScript
- Vite (build tool)

## Data Flow

### Create Product Example:
1. User fills form in `ProductsView.vue`
2. Component calls `productService.create()`
3. Axios sends POST to `/api/products`
4. `ProductsController.Create()` receives request
5. `ProductService.CreateProductAsync()` validates and processes
6. `IProductRepository.CreateAsync()` stores data
7. Response flows back through layers
8. UI updates with new product

## Key Design Patterns

- **Repository Pattern**: Abstracts data access
- **Unit of Work**: Manages transactions across repositories
- **Dependency Injection**: Used throughout backend
- **Service Layer**: Separates business logic from API layer
- **DTO Pattern**: Clean data transfer between layers
- **SPA Architecture**: Single-page application with Vue Router

## Configuration

### Backend (appsettings.json)
- Logging configuration
- CORS allowed origins
- Future: Database connection strings

### Frontend (api.ts)
- API base URL: `http://localhost:5000/api`
- Can be configured per environment

## Next Implementation Steps

1. Replace in-memory repositories with actual storage:
   - Create `EFCoreProductRepository` implementing `IProductRepository`
   - Create `EFCoreUnitOfWork` implementing `IUnitOfWork`
   - Add Entity Framework Core packages
   - Create DbContext
   - Update DI registration in Program.cs

2. Add proper validation:
   - FluentValidation for DTOs
   - Model validation in controllers

3. Add authentication:
   - JWT tokens
   - User management
   - Authorization policies

4. Improve error handling:
   - Global exception handler
   - Structured logging
   - User-friendly error messages
