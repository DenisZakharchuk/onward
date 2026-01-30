# Inventory System

A simple inventory management system for small businesses built with .NET 8 backend and Vue.js + Vite frontend.

## Project Structure

```
onward/
├── backend/                    # .NET Backend Solution
│   ├── InventorySystem.API/    # Web API (Presentation Layer)
│   ├── InventorySystem.Business/ # Business Logic Layer
│   ├── InventorySystem.DTOs/   # Data Transfer Objects
│   └── InventorySystem.DataAccess/ # Data Access Layer
└── frontend/                   # Vue.js Frontend
    └── src/
        ├── components/         # Vue Components
        ├── services/           # API Service Layer
        ├── types/              # TypeScript Types
        └── router/             # Vue Router
```

## Features

- **Product Management**: Create, read, update, and delete products with stock tracking
- **Category Management**: Organize products into categories
- **Stock Movements**: Track inventory changes (In, Out, Adjustment)
- **Low Stock Alerts**: Visual indicators for products below minimum stock
- **Clean Architecture**: Separated concerns with dedicated layers
- **Abstraction Layer**: Ready for multiple storage implementations

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Clean Architecture pattern
- Repository pattern with Unit of Work
- In-memory storage (placeholder for future implementations)

### Frontend
- Vue.js 3 with TypeScript
- Vite for fast development
- Vue Router for navigation
- Axios for HTTP requests
- Responsive design

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Visual Studio Code or Visual Studio 2022

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

4. Run the API:
```bash
cd InventorySystem.API
dotnet run
```

The API will start on `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).

API documentation is available at: `http://localhost:5000/swagger`

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`.

## API Endpoints

### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/low-stock` - Get low stock products
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Stock Movements
- `GET /api/stock` - Get all stock movements
- `GET /api/stock/product/{productId}` - Get movements for a product
- `POST /api/stock` - Create new stock movement

## Architecture

### Backend Architecture

The backend follows clean architecture principles with clear separation of concerns:

1. **InventorySystem.API** (Presentation Layer)
   - Controllers for handling HTTP requests
   - Dependency injection configuration
   - CORS setup for frontend communication

2. **InventorySystem.Business** (Business Logic Layer)
   - Services implementing business logic
   - Validation and business rules
   - Orchestration of data operations

3. **InventorySystem.DTOs** (Data Transfer Objects)
   - Request and response models
   - Shared between all layers
   - Clean data contracts

4. **InventorySystem.DataAccess** (Data Access Layer)
   - Domain models
   - Repository abstractions (interfaces)
   - Placeholder in-memory implementations
   - Unit of Work pattern for transactions

### Data Layer Abstraction

The data access layer is designed with abstraction in mind, allowing for multiple storage implementations:

- **Current**: In-memory repositories (for development and testing)
- **Future Options**:
  - SQL Server with Entity Framework Core
  - PostgreSQL
  - MongoDB
  - File-based storage
  - Any custom storage solution

To implement a new storage type:
1. Create implementations of `IProductRepository`, `ICategoryRepository`, `IStockMovementRepository`
2. Create an implementation of `IUnitOfWork`
3. Register the new implementations in `Program.cs`

### Frontend Architecture

The frontend is organized for scalability and maintainability:

- **Components**: Vue components for each view (Products, Categories, Stock)
- **Services**: API service layer for backend communication
- **Types**: TypeScript interfaces matching backend DTOs
- **Router**: Vue Router for SPA navigation

## Development Workflow

### Adding a New Feature

1. **Backend**:
   - Add DTOs in `InventorySystem.DTOs`
   - Update data models in `InventorySystem.DataAccess/Models`
   - Add repository methods to abstractions
   - Implement in placeholder repositories
   - Create/update service in `InventorySystem.Business/Services`
   - Add controller endpoint in `InventorySystem.API/Controllers`

2. **Frontend**:
   - Add TypeScript types in `src/types`
   - Create/update service methods in `src/services`
   - Update Vue components in `src/components`

### Testing the Application

1. Start the backend API
2. Start the frontend development server
3. Navigate to `http://localhost:5173`
4. Create some categories first
5. Add products to those categories
6. Create stock movements to track inventory changes

## Next Steps

This project is set up with placeholder abstractions for the data layer. The next steps would be:

1. **Database Implementation**: Replace in-memory repositories with actual database implementation (EF Core, Dapper, etc.)
2. **Authentication**: Add user authentication and authorization
3. **Reporting**: Add inventory reports and analytics
4. **Search & Filtering**: Enhanced product search and filtering capabilities
5. **Audit Trail**: Track changes and user actions
6. **API Validation**: Add comprehensive input validation
7. **Error Handling**: Implement global error handling and logging
8. **Unit Tests**: Add unit tests for business logic
9. **Integration Tests**: Add API integration tests

## License

This is a pet project for learning purposes.

## Contributing

Feel free to fork and experiment with this project!
