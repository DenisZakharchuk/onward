# Inventory System Project - Copilot Instructions

**📋 IMPORTANT: This project follows a comprehensive architecture defined in [Architecture.md](../Architecture.md). Please refer to it for complete backend rules, patterns, and guidelines.**

## Project Overview
Inventorization dashboard with microservices architecture:
- **Frontend**: Vue.js 3 + Vite with TypeScript
- **Backend**: Microservices using .NET 8 ASP.NET with Entity Framework & PostgreSQL
- **Data Storage**: PostgreSQL (primary), MongoDB (audit logs)
- **Message Broker**: Containerized message broker
- **Authorization**: JWT-based authentication via AuthService microservice

## Architecture Overview
See [Architecture.md](../Architecture.md) for complete architectural specifications including:
- Microservice structure and naming conventions
- Dependency injection and abstraction rules
- DTO typing and mapping strategies
- Testing requirements and patterns
- Domain service abstractions and entity modeling
- Base abstractions and shared project organization

## Code Generation
See [GENERATION.md](../GENERATION.md) for complete code generation patterns and metaprogramming approach:
- Template-based generation with Handlebars
- Metadata-driven development (JSON data models)
- Regeneration strategy (generated files can be overwritten; custom logic in services)
- Generator patterns and extension points
- Type safety enforcement (no `any` types)
- Template context patterns and naming conventions
- Generation phases and dependencies

**Tool Location**: `generation/code/` - TypeScript-based generator for BoundedContext scaffolding
**Custom Logic**: Add business logic in separate domain services, not in generated entities

## Backend Project Naming & Structure
Follow the conventions from Architecture.md strictly:
- **DTO Projects**: `Inventorization.[BoundedContextName].DTO` (class library)
- **Domain Projects**: `Inventorization.[BoundedContextName].Domain` (class library with Entities, Services, DbContexts, UOWs)
- **API Projects**: `Inventorization.[BoundedContextName].API` (ASP.NET web app)
- **Test Projects**: `Inventorization.[BoundedContextName].API.Tests` (unit tests required for every API project)

## Backend Key Requirements
1. **Shared Base Abstractions**: All base types (`CreateDTO`, `UpdateDTO`, `DeleteDTO`, `DetailsDTO`, `SearchDTO`, `PageDTO`, `ServiceResult<T>`, `IEntityCreator`, `IEntityModifier`, `ISearchQueryProvider`, `IMapper`, etc.) must be in `Inventorization.Base`
2. **Dependency Injection**: All dependencies injected as interfaces, never concrete types
3. **Data Services**: Must be generic, use `IMapper` abstraction for mapping/projection
4. **DTOs**: Structured with DTO/ subfolder (e.g., DTO/Customer), inherit from base DTOs
5. **Entity Mappers**: Use `IMapper<TEntity, TDetailsDTO>` for object mapping and LINQ projection
6. **Testing**: Every concrete abstraction must have unit test coverage
7. **Database**: PostgreSQL with Entity Framework; separate dedicated DB per microservice
8. **Authorization**: JWT bearer token auth; services may be anonymous where appropriate
9. **Documentation**: Swagger enabled in Development mode for all API projects

## 📝 Documentation Policy

### ✅ DO Create These Files
- **IMPLEMENTATION_PROGRESS.md** - Track ongoing work across sessions. Use this to continue development context when resuming work.
- **Architecture.md** - Core architectural specifications and rules
- **README.md** - Project setup and getting started
- **.md files for critical infrastructure** - Database schemas, deployment guides, security policies

### ❌ DO NOT Create These Files
- Summary or accomplishment markdown files (git commit messages + history provide this)
- Work session summaries (unnecessary, git log is the source of truth)
- Duplicate documentation (keep information in one authoritative location)
- Temporary progress notes (use IMPLEMENTATION_PROGRESS.md instead)

### Why This Approach
- **Git provides version history** - All changes are tracked in commits
- **IMPLEMENTATION_PROGRESS.md is unique** - It's living documentation for in-progress work, not a summary
- **Reduces git clutter** - Keeps only essential, reusable documentation
- **Single source of truth** - Architecture.md, not scattered summaries

## Key Features
- CRUD operations with structured DTOs and mappers
- Microservices architecture with dedicated data stores
- Async fire-and-forget audit logging (never blocks operations)
- GraphQL queries for audit logs with filtering
- MongoDB TTL index for automatic 90-day retention
- Large payload truncation (15MB limit)
- SOLID principles with abstraction-based design

## Checklist
- [x] Create copilot-instructions.md file
- [x] Scaffold .NET backend solution structure
- [x] Create Vue.js + Vite frontend
- [x] Setup backend projects and references
- [x] Implement domain models and abstractions
- [x] Create API controllers
- [x] Setup frontend structure and API integration
- [x] Create documentation
- [x] Add MongoDB + Apollo Server integration
- [x] Implement IAuditLogger abstraction (SOLID - Dependency Inversion)
- [x] Create MongoAuditLogger with async, truncation, TTL
- [x] Build GraphQL query layer with HotChocolate
- [x] Integrate audit logging into business services
- [x] Update VS Code tasks for MongoDB

## Current Status
✅ Backend audit logging complete! All CRUD operations log to MongoDB.

## Running the Application

### Full Stack (MongoDB + Backend + Frontend)
```bash
# Using VS Code Task
Run Task: "Start All"
```

### Individual Services

#### MongoDB
```bash
docker compose -f 'docker-compose.yml' up -d --build
```
MongoDB: mongodb://localhost:27017
Mongo Express: http://localhost:8081

#### Backend
```bash
cd backend/InventorySystem.API
dotnet run
```
API: http://localhost:5002
Swagger: http://localhost:5002/swagger
GraphQL: http://localhost:5002/graphql (Banana Cake Pop)

#### Frontend
```bash
cd frontend
npm run dev
```
Frontend: http://localhost:5173

## MongoDB Configuration
Connection string in `appsettings.json`:
```json
"MongoDB": {
  "ConnectionString": "mongodb://admin:admin123@localhost:27017",
  "DatabaseName": "inventorydb"
}
```

Can be overridden with environment variables:
- `MongoDB__ConnectionString`
- `MongoDB__DatabaseName`

## Audit Log Schema
```json
{
  "_id": ObjectId,
  "action": "ProductCreated|ProductUpdated|ProductDeleted|...",
  "entityType": "Product|Category|StockMovement",
  "entityId": "guid-string",
  "userId": "system",
  "ipAddress": null,
  "timestamp": ISODate,
  "changes": { ... },
  "metadata": { ... }
}
```

TTL Index: Documents auto-deleted after 90 days

## GraphQL Queries
Access Banana Cake Pop at http://localhost:5002/graphql

Example query:
```graphql
query {
  auditLogs(
    fromDate: "2025-01-01T00:00:00Z"
    toDate: "2025-12-31T23:59:59Z"
    entityType: "Product"
    action: "ProductCreated"
  ) {
    id
    action
    entityType
    entityId
    userId
    timestamp
    changes
  }
}
```

## Next Development Steps
1. ✅ ~~Replace in-memory storage with actual database (EF Core, Dapper, etc.)~~ *Partially - still using in-memory for inventory, MongoDB for audit*
2. Add authentication and authorization (extract real userId from HttpContext)
3. Implement IP address extraction from HttpContext
4. Frontend GraphQL integration (graphql-request + AuditLogView.vue)
5. Add unit and integration tests
6. Enhance UI/UX with better styling
7. Consider message broker (Kafka/RabbitMQ) for audit logging at scale

