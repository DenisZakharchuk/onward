# Inventorization Dashboard

An inventory management system with microservices architecture built with .NET 8 backend and Vue.js 3 + Quasar frontend.

## Project Structure

```
onward/
├── backend/                           # .NET Backend Microservices
│   ├── Inventorization.Base/          # Shared base abstractions and DTOs
│   ├── Inventorization.Auth.API/      # Auth microservice API (port 5012)
│   ├── Inventorization.Auth.Domain/   # Auth domain logic and entities
│   ├── Inventorization.Auth.DTO/      # Auth data transfer objects
│   ├── Inventorization.Goods.API/     # Goods microservice API (port 5022)
│   ├── Inventorization.Goods.Domain/  # Goods domain logic and entities
│   └── Inventorization.Goods.DTO/     # Goods data transfer objects
├── frontend/                          # Vue.js 3 + Quasar Frontend
│   └── quasar/                        # Quasar application
└── docker-compose.yml                 # Infrastructure services
```

## Architecture

This project follows **microservices architecture** with:
- **Bounded Contexts**: Auth, Goods (each with dedicated API, Domain, and DTO projects)
- **Database per Service**: Separate PostgreSQL databases for each microservice
- **JWT Authentication**: Token-based auth via Auth microservice
- **Entity Framework Core**: Code-first with migrations
- **Message Broker**: For inter-service communication
- **Audit Logging**: MongoDB-based audit trail

See [Architecture.md](Architecture.md) for complete architectural guidelines.

## Features

- **Authentication & Authorization**: User management, roles, permissions
- **Product Management**: Goods, categories, suppliers
- **Inventory Tracking**: Stock locations, warehouse management
- **Purchase Orders**: PO management and items
- **Audit Logging**: Complete audit trail in MongoDB
- **Swagger Documentation**: Auto-generated API docs for each service

## Technology Stack

### Backend
- .NET 8 ASP.NET Core Web API
- Entity Framework Core with PostgreSQL
- JWT Bearer Authentication
- FluentValidation for DTO validation
- Swagger/OpenAPI
- MongoDB for audit logs

### Frontend
- Vue.js 3 with TypeScript
- Quasar Framework
- Vite for fast development
- Vue Router
- Axios for HTTP requests

### Infrastructure
- PostgreSQL 16 (Auth DB, Goods DB)
- MongoDB 7.0 (Audit logs)
- Docker & Docker Compose
- PgAdmin for DB management
- Mongo Express for MongoDB management

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- Visual Studio Code (recommended)

### Quick Start

1. **Start all services** (easiest):
```bash
# Using VS Code Task
Run Task: "Start All"
```

Or manually:

### Infrastructure Setup

1. Start Docker daemon (Docker Desktop or systemctl)

2. Start infrastructure services:
```bash
docker compose up -d
```

This starts:
- PostgreSQL Auth: `localhost:5432`
- PostgreSQL Goods: `localhost:5433`
- MongoDB: `localhost:27017`
- PgAdmin: `http://localhost:5050`
- Mongo Express: `http://localhost:8081`

### Backend Setup

Each microservice can be started independently:

**Auth Service:**
```bash
cd backend/Inventorization.Auth.API
dotnet run
```
API: `http://localhost:5012`
Swagger: `http://localhost:5012/swagger`

**Goods Service:**
```bash
cd backend/Inventorization.Goods.API
dotnet run
```
API: `http://localhost:5022`
Swagger: `http://localhost:5022/swagger`

Or use VS Code tasks: "Start Auth Service", "Start Goods Service", or "Start Backend" (starts all)

### Frontend Setup

1. Navigate to the Quasar frontend directory:
```bash
cd frontend/quasar
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

Or use VS Code task: "Start Frontend"

The frontend will be available at `http://localhost:9000` (Quasar default).

## API Services & Endpoints

### Auth Service (Port 5012)

**Authentication:**
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/revoke` - Revoke refresh token

**Users:**
- `GET /api/users/search` - Search users (paginated)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

**Roles & Permissions:**
- `GET /api/roles/search` - Search roles
- `POST /api/roles` - Create role
- `PUT /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Delete role
- Similar endpoints for permissions

### Goods Service (Port 5022)

**Product Management:**
- `GET /api/goods/search` - Search goods (paginated)
- `GET /api/goods/{id}` - Get product details
- `POST /api/goods` - Create product
- `PUT /api/goods/{id}` - Update product
- `DELETE /api/goods/{id}` - Delete product

**Categories:**
- `GET /api/categories/search` - Search categories
- `GET /api/categories/{id}` - Get category
- CRUD operations similar to goods

**Suppliers, Warehouses, Stock, Purchase Orders:**
- Similar RESTful endpoints for each entity

All endpoints support:
- Pagination via SearchDTO
- Filtering and sorting
- JWT authentication (where required)

See Swagger docs for complete API reference:
- Auth: `http://localhost:5012/swagger`
- Goods: `http://localhost:5022/swagger`

## Development Workflow

### Adding a New Entity to a Bounded Context

Follow the established pattern (see [Architecture.md](Architecture.md) for details):

1. **Create Entity** in Domain/Entities with immutability pattern
2. **Create DTOs** (Create, Update, Delete, Details, Search) in DTO project
3. **Create Entity Configuration** in Domain/EntityConfigurations
4. **Create Mapper** implementing `IMapper<TEntity, TDetailsDTO>`
5. **Create Creator** implementing `IEntityCreator<TCreateDTO, TEntity>`
6. **Create Modifier** implementing `IEntityModifier<TEntity, TUpdateDTO>`
7. **Create Validator** for DTOs using FluentValidation
8. **Add Controller** extending DataController
9. **Register Services** in Program.cs
10. **Create Migration** and update database

### Adding a New Microservice (Bounded Context)

1. Create three projects:
   - `Inventorization.[Context].DTO` (class library)
   - `Inventorization.[Context].Domain` (class library)
   - `Inventorization.[Context].API` (ASP.NET web app)
   - `Inventorization.[Context].API.Tests` (test project)

2. Add PostgreSQL service to docker-compose.yml

3. Configure connection strings and JWT settings

4. Add VS Code tasks for the new service

5. Reference `Inventorization.Base` for shared abstractions

### Database Migrations

**Auth Service:**
```bash
cd backend/Inventorization.Auth.Domain
dotnet ef migrations add MigrationName --startup-project ../Inventorization.Auth.API
dotnet ef database update --startup-project ../Inventorization.Auth.API
```

**Goods Service:**
```bash
cd backend/Inventorization.Goods.Domain
dotnet ef migrations add MigrationName --startup-project ../Inventorization.Goods.API
dotnet ef database update --startup-project ../Inventorization.Goods.API
```

## Testing

### Running Tests

```bash
# All tests
dotnet test

# Specific test project
cd backend/Inventorization.Goods.API.Tests
dotnet test
```

### Manual Testing via Swagger

1. Start the infrastructure and services
2. Navigate to Swagger UI:
   - Auth: http://localhost:5012/swagger
   - Goods: http://localhost:5022/swagger
3. Test authentication flow:
   - Register a user via `/api/auth/register`
   - Login via `/api/auth/login` to get JWT token
   - Click "Authorize" in Swagger and paste the token
   - Test protected endpoints

## Database Management

**PostgreSQL (via PgAdmin):**
1. Open http://localhost:5050
2. Login: admin@example.com / admin
3. Add servers:
   - Auth DB: postgres-auth:5432
   - Goods DB: postgres-goods:5432

**MongoDB (via Mongo Express):**
1. Open http://localhost:8081
2. Browse `inventorydb` database
3. View audit logs in `AuditLogs` collection

## Project Status & Next Steps

See [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md) for current development status and pending tasks.

## Additional Documentation

- **[Architecture.md](Architecture.md)** - Complete architectural guidelines and patterns
- **[CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md)** - Controller design patterns
- **[METADATA_SYSTEM.md](METADATA_SYSTEM.md)** - Metadata and relationship management
- **[QUICKSTART.md](QUICKSTART.md)** - Quick start guide

## Contributing

When contributing to this project:
1. Follow the architectural guidelines in Architecture.md
2. Maintain the established patterns
3. Add unit tests for new features
4. Update documentation as needed
5. Test with all microservices running

## License

This project is for educational and development purposes.

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
