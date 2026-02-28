# Authorization in Code Generation — Design Plan

**Status**: Planning  
**Affects**: Blueprint schema, DomainModel schema, TypeScript models, Handlebars templates, .NET foundation projects, Inventorization.Auth.*

---

## 1. Current State Analysis

### Generation Tool

| Artifact | Current behaviour |
|---|---|
| `blueprint.schema.json` | No `authorization` concept — auth is implicitly "always JWT" |
| `data-model.schema.json` | No auth model — `jwt` node only emits appsettings values |
| `Blueprint.ts` / `DataModel.ts` | No auth-related TypeScript types |
| `controllers.generated.cs.hbs` | Always `[Authorize]` on every controller |
| `program/controllers.generated.cs.hbs` | Inline JWT bearer setup + `AddAuthorization()` — not extractable, duplicated across modes |
| `program/minimal.generated.cs.hbs` | Same problem |
| DI templates | No auth service registration |

### Auth .NET Projects

#### `Inventorization.Auth.BL` — observations & issues

| Item | Issue |
|---|---|
| `Services.Abstractions.IAuthorizationService` | **Naming conflict** with `Microsoft.AspNetCore.Authorization.IAuthorizationService` — using directives must disambiguate manually everywhere |
| `Services.Abstractions.IAuthorizationService` | Semantically duplicates ASP.NET policy engine; the current implementation is a thin pass-through to `IRolePermissionService`. Consider promoting `IRolePermissionService` as the primary surface |
| `JwtTokenProvider` reads `IConfiguration` directly | Should use the Options pattern (`JwtSettings` POCO + `IOptions<JwtSettings>`) for testability and consistency |
| `IUserRepository` registered twice in `Program.cs` (lines ~65 and ~173) | Duplicate DI registration — second one silently wins on Microsoft DI, but it is confusing |
| `Program.cs` is monolithic | All of JWT setup, SwaggerGen JWT config, CORS, seeding and service registration live in one file. Hard to unit-test, hard to reuse, hard to template |
| No `IServiceCollectionExtension` class like `OwnershipServiceCollectionExtensions` | Inconsistent with the Base.AspNetCore pattern already in place |

#### `Inventorization.Base.AspNetCore` — what exists and what is missing

Exists:
- `OwnershipServiceCollectionExtensions` — good reusable DI registration pattern
- `ClaimsCurrentUserService`, `HttpContextCurrentIdentityContext` — identity utilities

Missing (needed):
- `AuthServiceCollectionExtensions` — reusable JWT bearer + `AddAuthorization()` setup method
- `AuthMiddlewareExtensions` — reusable `UseAuthentication()` / `UseAuthorization()` pipeline setup
- `InventorizationAuthorizeAttribute` — thin wrapper over `[Authorize]` carrying resource+action metadata, enabling future policy-based extension without touching generated files
- `SwaggerJwtExtensions` — reusable Swagger + JWT security definition setup

---

## 2. Auth Resolution Modes

Three modes are defined in the **blueprint** (per bounded context):

| Mode | Meaning |
|---|---|
| `perDomain` | A dedicated external AuthService (`Inventorization.Auth.API`) handles all auth. The generated context validates JWT tokens issued by it; no local auth endpoints or entities |
| `perContext` | Auth endpoints (login/refresh/logout) and auth data entities live **inside** the generated bounded context, but the implementation is **reused** from `Inventorization.Auth.*` libs (not regenerated) |
| `none` | Authorization infrastructure is registered so templates remain agnostic, but all endpoints are `[AllowAnonymous]`. No JWT validation is required |

The mode is a **blueprint-level** decision because it is an architectural strategy choice, not a domain data model concern. The separation between blueprint and domain model is preserved.

---

## 3. Schema / Model Changes

### 3.1 Blueprint schema — new `authorization` section

```json
"authorization": {
  "type": "object",
  "required": ["mode"],
  "properties": {
    "mode": {
      "type": "string",
      "enum": ["perDomain", "perContext", "none"]
    },
    "perDomain": {
      "type": "object",
      "description": "Settings used when mode=perDomain",
      "required": ["authServiceUrl"],
      "properties": {
        "authServiceUrl": {
          "type": "string",
          "description": "Base URL of the AuthService instance (used to emit HttpClient registration)"
        }
      },
      "additionalProperties": false
    }
  },
  "if": { "properties": { "mode": { "const": "perDomain" } } },
  "then": { "required": ["perDomain"] },
  "additionalProperties": false
}
```

- `perContext` and `none` need no extra sub-object.
- If **omitted entirely**, the blueprint defaults to `none` (safest fallback; avoids breaking existing generated projects).

### 3.2 DomainModel schema — optional `authModel` on `BoundedContext`

Used **only** when the blueprint selects `perDomain` or `perContext`. Describes which roles/permissions the bounded context recognises, so generated controllers can emit typed `[Authorize(Policy = "...")]` attributes.

```json
"authModel": {
  "type": "object",
  "description": "Authorization model — which roles and resource-level permissions this context uses",
  "properties": {
    "provider": {
      "type": "string",
      "description": "Predefined auth provider family (e.g. 'Inventorization.Auth')",
      "enum": ["Inventorization.Auth"]
    },
    "roles": {
      "type": "array",
      "items": { "type": "string" },
      "description": "Named roles available to this context (emitted as string constants)"
    },
    "permissions": {
      "type": "object",
      "description": "Map of resource name → allowed actions (resource.action pairs become policy names)",
      "additionalProperties": {
        "type": "array",
        "items": { "type": "string" }
      }
    }
  },
  "additionalProperties": false
}
```

Example:
```json
"authModel": {
  "provider": "Inventorization.Auth",
  "roles": ["Admin", "Manager", "Viewer"],
  "permissions": {
    "Goods": ["Create", "Read", "Update", "Delete"],
    "Categories": ["Read"]
  }
}
```

### 3.3 TypeScript model types

Add to **`Blueprint.ts`**:

```typescript
export type AuthorizationMode = 'perDomain' | 'perContext' | 'none';

export interface BlueprintAuthorizationPerDomain {
  authServiceUrl: string;
}

export type BlueprintAuthorization =
  | { mode: 'none' }
  | { mode: 'perContext' }
  | { mode: 'perDomain'; perDomain: BlueprintAuthorizationPerDomain };

// Add to BlueprintBoundedContext:
export interface BlueprintBoundedContext {
  // ...existing...
  authorization?: BlueprintAuthorization;  // defaults to { mode: 'none' }
}
```

Add to **`DataModel.ts`**:

```typescript
export interface AuthModelConfig {
  provider: 'Inventorization.Auth';
  roles?: string[];
  permissions?: Record<string, string[]>;
}

// Add to BoundedContext:
export interface BoundedContext {
  // ...existing...
  authModel?: AuthModelConfig;
}
```

---

## 4. New .NET Foundation Classes

All classes go into **`Inventorization.Base.AspNetCore`** — the existing extension point for ASP.NET utilities. This keeps them reusable across all generated bounded contexts regardless of mode.

### 4.1 `InventorizationAuthorizeAttribute`

```csharp
// Inventorization.Base.AspNetCore/Authorization/InventorizationAuthorizeAttribute.cs
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class InventorizationAuthorizeAttribute : AuthorizeAttribute
{
    public InventorizationAuthorizeAttribute() { }

    public InventorizationAuthorizeAttribute(string resource, string action)
        : base(policy: $"{resource}.{action}")
    {
        Resource = resource;
        Action = action;
    }

    /// <summary>Resource name this attribute represents (e.g., "Goods").</summary>
    public string? Resource { get; }
    /// <summary>Action name this attribute represents (e.g., "Read").</summary>
    public string? Action { get; }
}
```

**Rationale**: Generated controllers use `[InventorizationAuthorize]` instead of `[Authorize]`. This:
- Is template-agnostic to auth mode (even `none` still compiles — the anonymous pipeline just never challenges)
- Carries resource+action metadata for future policy-engine extension
- Avoids touching every controller when the auth mechanism evolves

### 4.2 `AuthServiceCollectionExtensions`

```csharp
// Inventorization.Base.AspNetCore/Extensions/AuthServiceCollectionExtensions.cs
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT Bearer authentication and AddAuthorization().
    /// Suitable for both perDomain and perContext modes — accepts pre-built TokenValidationParameters.
    /// </summary>
    public static IServiceCollection AddInventorizationJwtAuth(
        this IServiceCollection services,
        TokenValidationParameters tokenValidationParameters)
    { ... }

    /// <summary>
    /// Convenience overload: reads JwtSettings section from configuration.
    /// </summary>
    public static IServiceCollection AddInventorizationJwtAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "JwtSettings")
    { ... }

    /// <summary>
    /// Registers AddAuthentication + AddAuthorization as anonymous-only (mode=none).
    /// No JWT validation. All policies pass through by default.
    /// </summary>
    public static IServiceCollection AddInventorizationAnonymousAuth(
        this IServiceCollection services)
    { ... }
}
```

### 4.3 `AuthMiddlewareExtensions`

```csharp
// Inventorization.Base.AspNetCore/Extensions/AuthMiddlewareExtensions.cs
public static class AuthMiddlewareExtensions
{
    /// <summary>
    /// Adds UseAuthentication() + UseAuthorization() in the correct order.
    /// Replaces direct calls in generated Program.cs.
    /// </summary>
    public static IApplicationBuilder UseInventorizationAuth(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
```

### 4.4 `SwaggerAuthExtensions`

```csharp
// Inventorization.Base.AspNetCore/Extensions/SwaggerAuthExtensions.cs
public static class SwaggerAuthExtensions
{
    /// <summary>
    /// Adds the JWT Bearer security definition and global security requirement to SwaggerGen.
    /// </summary>
    public static SwaggerGenOptions AddJwtBearerSecurity(this SwaggerGenOptions options) { ... }
}
```

### 4.5 `JwtSettings` options POCO

```csharp
// Inventorization.Base.AspNetCore/Auth/JwtSettings.cs
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
```

Move `JwtTokenProvider` to consume `IOptions<JwtSettings>` rather than `IConfiguration` directly.

---

## 5. Auth.BL Project Improvements

### 5.1 Rename `IAuthorizationService` → `IInventorizationAuthorizationService`

The current name directly conflicts with `Microsoft.AspNetCore.Authorization.IAuthorizationService`. Every file that uses both needs explicit aliases. Rename to eliminate the ambiguity:

```csharp
// Before
public interface IAuthorizationService { ... }

// After
public interface IInventorizationAuthorizationService { ... }
```

Update: `AuthorizationService.cs` (impl), `Program.cs` registration, any callers.

### 5.2 Remove duplicate `IUserRepository` registration in `Program.cs`

Lines ~65 and ~173 both register `Services.Abstractions.IUserRepository`. Remove the second registration (or consolidate into the `AuthServiceCollectionExtensions` class).

### 5.3 Extract `AuthBLServiceCollectionExtensions`

Move auth BL registrations into a reusable extension method in `Inventorization.Auth.BL`:

```csharp
// Inventorization.Auth.BL/Extensions/AuthBLServiceCollectionExtensions.cs
public static class AuthBLServiceCollectionExtensions
{
    public static IServiceCollection AddAuthBusinessServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        // DbContext, repositories, UoW, data services, JWT token provider, auth services...
    }
}
```

`Program.cs` becomes:
```csharp
builder.Services.AddAuthBusinessServices(opt => opt.UseNpgsql(connectionString));
builder.Services.AddInventorizationJwtAuth(builder.Configuration);
```

### 5.4 Adopt Options pattern in `JwtTokenProvider`

```csharp
// Before: IConfiguration _config
// After: IOptions<JwtSettings> _jwtOptions
```

Register via:
```csharp
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
```

---

## 6. Template Changes

### 6.1 New template structure

```text
templates/
  api/
    auth/
      jwt-registration.hbs          ← JWT DI block (perDomain / perContext)
      anonymous-registration.hbs    ← No-op auth DI block (none)
      per-context-auth-endpoints.hbs ← Minimal API delegates for login/refresh/logout (perContext)
    program/
      controllers.generated.cs.hbs  ← updated: uses auth partial + UseInventorizationAuth()
      minimal.generated.cs.hbs      ← updated: same
```

### 6.2 `controllers.generated.cs.hbs` — before vs after

**Before** (inline JWT block, repeated in every program template):
```csharp
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw ...
builder.Services.AddAuthentication(...)
    .AddJwtBearer(options => { ... TokenValidationParameters ... });
builder.Services.AddAuthorization();
```

**After** (single call to extracted extension):
```csharp
{{#if (eq authMode "none")}}
builder.Services.AddInventorizationAnonymousAuth();
{{else}}
builder.Services.AddInventorizationJwtAuth(builder.Configuration);
{{/if}}
```

And in the pipeline section:
```csharp
// Before
app.UseAuthentication();
app.UseAuthorization();

// After
app.UseInventorizationAuth();
```

### 6.3 `crud.generated.cs.hbs` — controller attribute

```csharp
// Before
[Authorize]

// After
{{#if (eq authMode "none")}}
[AllowAnonymous]
{{else}}
[InventorizationAuthorize]
{{/if}}
```

Template context gains `authMode: string` from the blueprint's `authorization.mode`.

### 6.4 `per-context-auth-endpoints.hbs` (new, perContext mode only)

Emits minimal API endpoint delegates that wire up `IAuthenticationService` from `Inventorization.Auth.BL`:  
- `POST /auth/login` → `IAuthenticationService.LoginAsync`
- `POST /auth/refresh` → `IAuthenticationService.RefreshTokenAsync`  
- `POST /auth/logout` → `IAuthenticationService.LogoutAsync`

These are **not** generated from the domain model (auth entities are not in `boundedContext.dataModel`). They are emitted as a fixed partial when `perContext` mode is selected.

### 6.5 csproj template — conditional `Inventorization.Auth.*` references

When blueprint `authorization.mode == "perContext"`:
```xml
<ProjectReference Include="..\Inventorization.Auth.BL\Inventorization.Auth.BL.csproj" />
<ProjectReference Include="..\Inventorization.Auth.DTO\Inventorization.Auth.DTO.csproj" />
```

---

## 7. Generation Tool Changes (TypeScript)

### 7.1 Resolve auth mode from blueprint

Add a helper in the generator orchestration layer:

```typescript
// src/utils/AuthModeResolver.ts
export function resolveAuthMode(blueprint: Blueprint): AuthorizationMode {
  return blueprint.boundedContext.authorization?.mode ?? 'none';
}
```

### 7.2 Pass `authMode` into template contexts

All program and controller template contexts receive:
```typescript
interface AuthTemplateContext {
  authMode: 'perDomain' | 'perContext' | 'none';
  authServiceUrl?: string;  // only when perDomain
}
```

Inject via `ProgramGenerator.buildContext()` and `ControllerGenerator.buildContext()`.

### 7.3 `ProgramGenerator` — select auth partial by mode

```typescript
const authPartial = authMode === 'none'
  ? 'api/auth/anonymous-registration.hbs'
  : 'api/auth/jwt-registration.hbs';
```

### 7.4 `csprojGenerator` — conditionally add Auth project references

When `authMode === 'perContext'`, add `Inventorization.Auth.BL` and `Inventorization.Auth.DTO` `<ProjectReference>` entries.

### 7.5 New optional per-context endpoints generator

`PerContextAuthEndpointsGenerator` runs **only** when `authMode === 'perContext'`. Emits the `auth/per-context-auth-endpoints.hbs` partial appended into the program file or as a separate `AuthEndpoints.cs` in the API project.

---

## 8. Implementation Order

| Step | What | Who (layer) |
|---|---|---|
| 1 | Rename `IAuthorizationService` → `IInventorizationAuthorizationService` in Auth.BL | .NET |
| 2 | Extract `AuthBLServiceCollectionExtensions` in Auth.BL | .NET |
| 3 | Introduce `JwtSettings` POCO + update `JwtTokenProvider` to use `IOptions<JwtSettings>` | .NET |
| 4 | Add `InventorizationAuthorizeAttribute` to Base.AspNetCore | .NET |
| 5 | Add `AuthServiceCollectionExtensions` + `AuthMiddlewareExtensions` + `SwaggerAuthExtensions` to Base.AspNetCore | .NET |
| 6 | Fix duplicate `IUserRepository` registration in Auth.API/Program.cs | .NET |
| 7 | Update `blueprint.schema.json` with `authorization` property | Schema |
| 8 | Update `data-model.schema.json` with `authModel` on BoundedContext | Schema |
| 9 | Update `Blueprint.ts` with auth types | TypeScript |
| 10 | Update `DataModel.ts` with `AuthModelConfig` | TypeScript |
| 11 | Add `AuthModeResolver` utility | TypeScript |
| 12 | Create new auth Handlebars partials (`jwt-registration.hbs`, `anonymous-registration.hbs`, `per-context-auth-endpoints.hbs`) | Templates |
| 13 | Update `controllers.generated.cs.hbs` and `program/`.hbs files to use partials + `InventorizationAuthorize` | Templates |
| 14 | Update `ProgramGenerator` to inject `authMode` into context and select auth partial | TypeScript/Generator |
| 15 | Update `ControllerGenerator` to inject `authMode` and emit correct attribute | TypeScript/Generator |
| 16 | Conditional `<ProjectReference>` in `csproj` templates for `perContext` mode | Templates/Generator |
| 17 | Add `PerContextAuthEndpointsGenerator` | TypeScript/Generator |
| 18 | Update schema validation tests and generator unit tests | Tests |

---

## 9. Open Questions / Decisions Needed

1. **`perContext` + `perDomain` simultaneously** — is this a legal combination? (e.g., a context that has its own auth AND validates external tokens). Current proposal: mutually exclusive modes; choose one.

2. **`authModel.permissions` at controller level** — should `[InventorizationAuthorize("Goods", "Create")]` be generated on individual action methods, or only at the controller class level? Per-action is more granular but more verbose in templates.

3. **Swagger security requirement when `none`** — should Swagger still show the Bearer auth UI in `none` mode? Proposal: no — skip `AddSecurityRequirement` when anonymous.

4. **perContext DB migration** — `perContext` mode imports `Inventorization.Auth.BL` which carries its own `AuthDbContext`. Should the generated bounded context share the same DB as Auth entities, or create a separate DB? Recommendation: separate DB via a second connection string, or reuse Auth DB via separate migration path.

5. **Future auth providers** — `authModel.provider` is currently `enum: ["Inventorization.Auth"]`. Keeping it as an enum allows adding new providers (e.g., `"Keycloak"`, `"AzureAD"`) later without breaking existing models.
