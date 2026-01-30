# Inventory System Project - Copilot Instructions

## Project Overview
Simple inventorisation system for small businesses with .NET 8 backend, Vue.js frontend, and MongoDB audit logging.

## Architecture
- Backend: .NET 8 with clean architecture (API, Business Logic, DTOs, Data Access, Audit Log)
- Frontend: Vue.js 3 + Vite with TypeScript
- Data Layer: Abstraction-based design supporting multiple storage types
- Audit Log: MongoDB with GraphQL query layer

## Project Structure
- `backend/` - .NET 8 solution with 5 projects
  - `InventorySystem.API` - REST API + GraphQL endpoint at /graphql
  - `InventorySystem.Business` - Business services with audit logging
  - `InventorySystem.DTOs` - Data transfer objects
  - `InventorySystem.DataAccess` - Repository abstractions and implementations
  - `InventorySystem.AuditLog` - MongoDB audit logger with TTL (90 days)
- `frontend/` - Vue.js + Vite application
- `docker-compose.yml` - MongoDB and Mongo Express containers

## Key Features
- CRUD operations for Products, Categories, and Stock Movements
- Async fire-and-forget audit logging (never blocks operations)
- GraphQL queries for audit logs with filtering
- MongoDB TTL index for automatic 90-day retention
- Large payload truncation (15MB limit)
- SOLID principles with IAuditLogger abstraction

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
docker-compose up -d
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

