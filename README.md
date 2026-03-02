# Onward

A microservices platform framework built with .NET 8 backend services, Vue 3 + Quasar frontend, and a TypeScript code generator that scaffolds entire bounded contexts from a JSON data model.

*Inventorization (goods, categories, suppliers, stock) is used as a concrete example domain to drive real implementation — it is not the goal of the project.*

---

## Table of Contents

1. [Overview](#overview)
2. [Repository Structure](#repository-structure)
3. [Technology Stack](#technology-stack)
4. [Bounded Contexts (Microservices)](#bounded-contexts-microservices)
5. [Code Generator](#code-generator)
6. [Authentication & Authorization](#authentication--authorization)
7. [Infrastructure](#infrastructure)
8. [Getting Started](#getting-started)
9. [Running Services](#running-services)
10. [Database Migrations](#database-migrations)
11. [Testing](#testing)
12. [Key Documentation](#key-documentation)

---

## Overview

Onward is a framework for building independently deployable bounded-context microservices. Each service owns a dedicated PostgreSQL database, exposes a RESTful API with Swagger, and produces structured audit logs to MongoDB.

The core value is the **metadata-first, code-generation** approach. A data model JSON file describes entities, relationships, and architecture slots; the TypeScript generator scaffolds the full microservice (solution projects, EF Core entities, CRUD services, ADT-based query layer, DI wiring, tests, and more) from Handlebars templates.

The inventorization domain (Auth, Goods) serves as the working reference implementation — a real, non-trivial bounded context that exercises every feature of the framework and generator.

---

## Repository Structure

```
onward/
├── backend/                              # All .NET projects (single solution: onward.sln)
│   ├── Onward.Base/                      # Shared abstractions (DTOs, interfaces, base classes)
│   ├── Onward.Base.API/                  # Shared API base classes (DataController, BaseQueryController)
│   ├── Onward.Base.AspNetCore/           # ASP.NET middleware, JWT auth, gRPC client, policy engine
│   │
│   ├── Onward.Auth.API/                  # Auth service — port 5012
│   ├── Onward.Auth.BL/                   # Auth domain (Users, Roles, Permissions, RefreshTokens)
│   ├── Onward.Auth.DTO/                  # Auth DTOs
│   ├── Onward.Auth.API.Tests/            # Auth unit tests
│   │
│   ├── Inventorization.Goods.API/        # Goods service — port 5022
│   ├── Inventorization.Goods.BL/         # Goods domain (Goods, Categories, Suppliers, …)
│   ├── Inventorization.Goods.DTO/        # Goods DTOs
│   ├── Inventorization.Goods.Common/     # Goods enums / constants
│   └── Inventorization.Goods.API.Tests/  # Goods unit tests
│
├── frontend/
│   └── quasar/                           # Vue 3 + Quasar + TypeScript SPA
│
├── generation/
│   └── code/                             # TypeScript BoundedContext code generator
│       ├── src/                          # Generator source (generators, utils, templates)
│       ├── templates/                    # Handlebars templates (.hbs)
│       ├── examples/                     # Example data-model + blueprint JSON files
│       └── schemas/                      # JSON Schema for data-model + blueprint validation
│
├── Architecture.md                       # Comprehensive backend architecture rules
├── GENERATION.md                         # Code generator patterns and template guide
├── docker-compose.yml                    # Infrastructure containers
└── onward.sln                            # .NET solution file
```

---

## Technology Stack

### Backend
| Concern | Technology |
|---|---|
| Runtime | .NET 8 / ASP.NET Core |
| ORM | Entity Framework Core 8 (Npgsql) |
| Primary DB | PostgreSQL 16 (one database per bounded context) |
| Audit DB | MongoDB 7.0 |
| Auth | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Online introspection | HTTP or gRPC (`Grpc.AspNetCore 2.65.0`) |
| API docs | Swagger / Swashbuckle |
| Tests | xUnit + FluentAssertions + Moq + EF Core InMemory |

### Frontend
| Concern | Technology |
|---|---|
| Framework | Vue 3 (Composition API) + TypeScript |
| UI Kit | Quasar Framework |
| Build | Vite |
| HTTP | Axios |

### Code Generator
| Concern | Technology |
|---|---|
| Language | TypeScript (Node.js) |
| Templates | Handlebars |
| Validation | AJV (JSON Schema) |
| CLI | yargs |

### Infrastructure
| Service | Port | Purpose |
|---|---|---|
| `postgres-auth` | 5432 | Auth service database |
| `postgres-goods` | 5433 | Goods service database |
| `pgadmin` | 5050 | PostgreSQL UI |
| `mongodb` | 27017 | Audit log storage |
| `mongo-express` | 8081 | MongoDB UI |

---

## Bounded Contexts (Microservices)

### Auth Service — `Onward.Auth.API` (port 5012)

Manages identity: users, roles, permissions, and JWT issuance/revocation.

Key capabilities:
- `POST /api/auth/login` — issue access + refresh tokens
- `POST /api/auth/refresh` — rotate refresh token
- `POST /api/tokens/introspect` — per-request token introspection for online mode
- gRPC `AuthIntrospection.IntrospectToken` — high-throughput introspection over gRPC
- Full CRUD for Users, Roles, Permissions with many-to-many relationship managers
- Swagger: `http://localhost:5012/swagger`

### Goods Service — `Inventorization.Goods.API` (port 5022)

Reference implementation of a domain bounded context (goods, categories, suppliers, stock locations, purchase orders). Demonstrates all generator output patterns in production-quality code.

Key capabilities:
- CRUD + ADT-based semantic query endpoints for every entity
- JWT authentication via `perDomain/local` mode
- Swagger: `http://localhost:5022/swagger`

---

## Code Generator

The generator (`generation/code/`) scaffolds a complete bounded context from two JSON files:

| Input | Purpose |
|---|---|
| **Data model** (`*.json`) | Entities, properties, relationships, auth model |
| **Blueprint** (`*.json`) | Architecture slots (ORM kind, DTO style, auth mode, API style) |

### What Gets Generated

For every bounded context and each entity inside it:

- Solution projects: `.DTO`, `.BL`, `.Common`, `.Meta`, `.API`, `.DI`, `.API.Tests`
- `GlobalUsings.cs`, `.csproj` files with correct references and NuGet packages
- EF Core entities (immutability pattern), `DbContext`, `IEntityTypeConfiguration<T>` classes
- `UnitOfWork` inheriting `UnitOfWorkBase<TDbContext>`
- `DataServiceBase`-derived data services + creator / modifier / mapper / search-provider abstractions
- `DataController<…>` and `BaseQueryController<…>` concrete controllers
- ADT-based query layer: `QueryBuilder`, `SearchService`, `ProjectionMapper`, `SearchQueryValidator`
- DI extension: `Add{Context}Services(IServiceCollection, IConfiguration)`
- `appsettings.json` / `appsettings.Development.json` with per-service connection strings
- Auth endpoints (`AuthController`, auth DTOs, `I{Context}AuthenticationService` interface) when `mode: "perContext"`
- Full xUnit test suite (~211+ tests per entity set)

### CLI Usage

```bash
cd generation/code
npm run build

# Dry run (preview, no files written)
node dist/cli.js generate examples/simple-bounded-context.json \
  --output-dir ../../backend \
  --blueprint examples/default-blueprint.json \
  --dry-run

# Generate
node dist/cli.js generate examples/simple-bounded-context.json \
  --output-dir ../../backend \
  --blueprint examples/default-blueprint.json

# Validate data model only
node dist/cli.js validate examples/simple-bounded-context.json
```

Or use VS Code tasks: **Generator: Build**, **Generator: Generate**, **Generator: Generate (dry-run)**.

See [GENERATION.md](GENERATION.md) for the full template and generator guide.

---

## Authentication & Authorization

### Auth Modes (configured per bounded context in the blueprint)

| Blueprint `mode` / `authMode` | Behaviour | `Program.cs` call |
|---|---|---|
| `perDomain` / `local` *(default)* | JWT validated locally | `AddOnwardJwtAuth` |
| `perDomain` / `online` | JWT validated + per-request introspection against Auth service | `AddOnwardOnlineAuth` |
| `perContext` | BC issues its own tokens; auth endpoints generated in the BC | `AddOnwardJwtAuth` + `PerContextAuthEndpointsGenerator` |
| `none` | All endpoints `[AllowAnonymous]` | `AddOnwardAnonymousAuth` |

### Online Introspection Transport

```json
"OnlineAuth": {
  "AuthServiceBaseUrl": "http://auth-service:5012",
  "Transport": "Http",       // or "Grpc"
  "CacheTtlSeconds": 30,
  "FailOpen": false
}
```

Both HTTP and gRPC transports are wrapped with `CachedAuthIntrospectionClient` for per-JTI TTL caching.

### `[OnwardAuthorize]` Attribute

```csharp
[OnwardAuthorize]                          // any authenticated user
[OnwardAuthorize("Product")]               // any action on Product resource
[OnwardAuthorize("Product", "Read")]       // specific action guard
```

The attribute sets `Policy = "Resource.Action"`. `OnwardPermissionPolicyProvider` resolves this at runtime; `OnwardPermissionAuthorizationHandler` evaluates Admin-bypass → role claims → permission claims.

### Tenant Scoping (opt-in)

```csharp
builder.Services.AddOnwardTenantScoping();
// + implement ITenantScopeFilter<TEntity> and register it
```

`DataServiceBase.SearchAsync` applies the registered `ITenantScopeFilter<TEntity>` automatically when a `tenant_id` JWT claim is present.

---

## Infrastructure

Start all containers:

```bash
docker compose up -d
```

| URL | Service |
|---|---|
| `localhost:5432` | PostgreSQL — Auth DB |
| `localhost:5433` | PostgreSQL — Goods DB |
| `http://localhost:5050` | PgAdmin (admin@example.com / admin) |
| `localhost:27017` | MongoDB (admin / admin123) |
| `http://localhost:8081` | Mongo Express |

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+ and npm
- Docker and Docker Compose
- VS Code (recommended — tasks and launch configs included)

### Full Stack (one command)

Open the workspace in VS Code and run the **Start All** task, which starts infrastructure, Auth service, Goods service, and the frontend in the correct order.

### Manual Setup

**1. Start infrastructure**
```bash
docker compose up -d
```

**2. Start backend services**
```bash
# Auth service
ASPNETCORE_URLS=http://localhost:5012 dotnet run --project backend/Onward.Auth.API

# Goods service (separate terminal)
ASPNETCORE_URLS=http://localhost:5022 dotnet run --project backend/Inventorization.Goods.API
```

**3. Start frontend**
```bash
cd frontend/quasar
npm install
npm run dev
```

---

## Running Services

| VS Code Task | What it does |
|---|---|
| `Start Infra` | `docker compose up -d` |
| `Start Auth Service` | `dotnet run` on port 5012 (depends on Infra) |
| `Start Goods Service` | `dotnet run` on port 5022 (depends on Infra) |
| `Start Backend` | Both services in parallel |
| `Start Frontend` | `npm run dev` in `frontend/quasar` |
| `Start All` | Everything above |
| `Debug Auth Service` | Auth service in Debug mode |
| `Debug Goods Service` | Goods service in Debug mode |
| `Stop Infra` | `docker compose down` |
| `Generator: Build` | Build the TypeScript generator |
| `Generator: Generate` | Run the generator (prompts for paths) |
| `Generator: Generate (dry-run)` | Preview generation output without writing files |
| `Generator: Validate` | Validate a data model JSON file |

---

## Database Migrations

```bash
# Auth service
cd backend/Onward.Auth.BL
dotnet ef migrations add <Name> --startup-project ../Onward.Auth.API
dotnet ef database update --startup-project ../Onward.Auth.API

# Goods service
cd backend/Inventorization.Goods.BL
dotnet ef migrations add <Name> --startup-project ../Inventorization.Goods.API
dotnet ef database update --startup-project ../Inventorization.Goods.API
```

---

## Testing

```bash
# All projects
dotnet test onward.sln

# Single test project
dotnet test backend/Inventorization.Goods.API.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

Tests use xUnit + FluentAssertions, Moq, and EF Core InMemory (no mocked `IQueryable`).

---

## Key Documentation

| Document | Contents |
|---|---|
| [Architecture.md](Architecture.md) | Complete backend rules: entity patterns, base classes, DI, testing, relationship managers, auth infrastructure, query architecture |
| [GENERATION.md](GENERATION.md) | Code generator internals: template guide, blueprint schema, generator pipeline, auth mode system |
| [CONTROLLER_ARCHITECTURE.md](CONTROLLER_ARCHITECTURE.md) | `DataController<…>` and `BaseQueryController<…>` design |
| [METADATA_SYSTEM.md](METADATA_SYSTEM.md) | `DataModelMetadata`, `DataModelRelationships`, metadata-driven validation |
| [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md) | Current development status and in-progress work |
| [QUICKSTART.md](QUICKSTART.md) | Minimal steps to get running |


