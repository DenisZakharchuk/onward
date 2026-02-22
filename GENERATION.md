# Code Generation Patterns & Metaprogramming Approach

**Location**: `generation/code/`  
**Language**: TypeScript  
**Purpose**: Automated scaffolding of BoundedContext microservices from metadata

---

## Overview

This document defines the patterns, principles, and architecture for the code generation tool that automates creation of BoundedContext projects following the patterns defined in [Architecture.md](Architecture.md).

### SOLID Principles

The generator strictly follows SOLID principles:

- **Single Responsibility**: Each generator handles one type of code (entities, DTOs, etc.)
- **Open/Closed**: Extensible through new generators without modifying existing ones
- **Liskov Substitution**: All generators implement `IGenerator` interface  
- **Interface Segregation**: Focused interfaces (`IGenerator`, `IModelProvider`, `IResultWriter`)
- **Dependency Inversion**: High-level modules depend on abstractions, not implementations

### Dependency Injection Pattern

Uses **constructor injection** with a composition root pattern:

```typescript
// Composition Root (CLI)
const modelProvider = new FileModelProvider();
const resultWriter = new FileResultWriter(outputDir);
const orchestrator = new Orchestrator(resultWriter, options);

// DomainModel loaded from JSON; Orchestrator loops over all bounded contexts
const domain: DomainModel = await modelProvider.load(source);
await orchestrator.generate(domain);
```

**Key Abstractions**:
- `IModelProvider` - Loads and validates data models (currently from files, future: API, DB)
- `IResultWriter` - Writes generated output (currently file system, future: HTTP, memory)
- `IGenerator` - Transitional legacy generator contract
- `GeneratorADT` contracts - ADT-oriented generator taxonomy (`deterministic | variant | composite | optional`)

### Implementation Status (Feb 2026)

Current implementation uses ADT-compatible orchestration with legacy compatibility:

- Orchestrator executes through `GeneratorRegistry` using explicit phase/dependency descriptors
- Existing generators run via `LegacyGeneratorAdapter`
- `IGenerator` remains supported but is transitional for new design work

DTO generation supports bounded layout variants:

- `boundedContext.dtoLayout = "class" | "record"`
- Default layout is `"class"`
- Selection is performed by `DtoVariantSelector`

Blueprint-driven architecture selection is implemented end-to-end:

- CLI supports `--blueprint` for `validate` and `generate`
- Blueprint schema is validated via `blueprint.schema.json`
- Execution slots are resolved from blueprint (`presentation`, `dataAccess`, `dto`)
- Conflict policy is **fail-on-conflict** (for example, `data-model.dtoLayout` vs blueprint DTO style)
- `grpc` presentation is currently defined in schema but intentionally **not implemented** (fail-fast)

Template migration status:

- Generators now use **subdirectory-first template lookup**
- Legacy flat template names remain as fallback compatibility path
- New structure is active across DTO, API, DI, Domain, Query, Metadata, Project, and Tests

## Philosophy

### Metadata-Driven Development

The generator follows a **metadata-first approach** where a single JSON data model serves as the source of truth for:

- Entity structure and properties
- Relationships between entities
- Validation rules
- Database configurations
- API surface area

**Benefits**:
- Single source of truth eliminates inconsistencies
- Changes to data model propagate automatically
- Documentation extracted from metadata
- Type safety enforced at metadata level

### Template-Based Generation

Uses **Handlebars templates** for code generation:

```
Metadata (JSON) → Template Context → Handlebars → Generated Code (.cs)
```

**Why Handlebars**:
- Industry standard with mature tooling
- Clear separation of logic and templates
- Readable by non-developers
- Supports custom helpers for complex transformations

### Template Subdirectory Convention

Blueprint variants are organized by **concern + variant**. Current canonical structure under `generation/code/templates/`:

```text
templates/
  api/
    controllers/
      crud.generated.cs.hbs
      query.generated.cs.hbs
    endpoints/
      minimal.generated.cs.hbs
    program/
      controllers.generated.cs.hbs
      controllers.ado-net.generated.cs.hbs
      minimal.generated.cs.hbs
      minimal.ado-net.generated.cs.hbs

  common/
    enum.hbs

  di/
    service-collection/
      ef-core.generated.cs.hbs
      ado-net.generated.cs.hbs

  domain/
    abstractions/
      creator.generated.cs.hbs
      modifier.generated.cs.hbs
      mapper.generated.cs.hbs
      search-provider.generated.cs.hbs
    configuration/
      entity.generated.cs.hbs
      junction.generated.cs.hbs
    data-service/
      generated.cs.hbs
    db-context/
      ef-core.generated.cs.hbs
    entity/
      regular.hbs
      junction.hbs
    repository/
      ado-net.generated.cs.hbs
    unit-of-work/
      ef-core.generated.cs.hbs
      ado-net.generated.cs.hbs
    validator/
      create.generated.cs.hbs
      update.generated.cs.hbs

  dto/
    class/
      create-dto.hbs
      update-dto.hbs
      delete-dto.hbs
      init-dto.hbs
      details-dto.hbs
      search-dto.hbs
    record/
      create-dto.hbs
      update-dto.hbs
      delete-dto.hbs
      init-dto.hbs
      details-dto.hbs
      search-dto.hbs
    projection.generated.cs.hbs

  metadata/
    data-model.generated.cs.hbs
    relationships.generated.cs.hbs

  project/
    csproj/
      api.hbs
      common.hbs
      di.hbs
      domain.hbs
      dto.hbs
      meta.hbs
      tests.hbs
    global-usings.hbs

  query/
    projection/
      mapper-interface.generated.cs.hbs
      mapper.generated.cs.hbs
    search/
      fields.generated.cs.hbs
      query-validator.generated.cs.hbs
      service.generated.cs.hbs
    query-builder.generated.cs.hbs

  tests/
    data-service.generated.cs.hbs
    validator.generated.cs.hbs
    mapper.generated.cs.hbs
    search-service.generated.cs.hbs
    instantiation.generated.cs.hbs
```

Rules:

- First level = bounded concern (`dto`, `domain`, `api`, `di`, `project`, `tests`, `metadata`, `common`, `query`).
- Second level = variant slot where applicable (`class|record`, `ef-core|ado-net`, `controllers|minimal-api`).
- Keep filenames stable across variants where semantics match (for example both DTO styles use `create-dto.hbs`).
- Use `.generated` in template filename only when output file is generation-owned and expected to be overwritten.

Migration guidance:

- Move templates incrementally by concern.
- Use new path first, legacy flat filename second during transition.
- Once migration is complete and old files are removed, keep fallback only if backward compatibility is required.

Example fallback call pattern:

```typescript
await this.writeRenderedTemplate(
  ['domain/validator/create.generated.cs.hbs', 'create-validator.generated.cs.hbs'],
  context,
  filePath,
  true
);
```

### Generation Stamp

**Purpose**: Every generation run produces a unique **generationStamp** (16-character identifier) that's embedded in all generated files.

**Format**: `YYYYMMDD-XXXXXXXX` (datestamp + 8-char hash)

**Usage**:
- Generated once per generation run by Orchestrator
- Embedded in header comments of all generated files
- Enables tracking which files belong to the same generation batch
- Helps identify outdated files from previous generations

**Example**:
```csharp
// <auto-generated>
//     Generation Stamp: 20260208-a4f3c891
//     Generated: 2026-02-08 14:23:45 UTC
//     Source: product-context.json
// </auto-generated>
```

### Generated vs Custom Code Strategy

**Generated Code**: `.cs` files in BL/DTO projects (can be regenerated/overwritten)  
**Custom Logic**: Separate services, controllers, or domain service extensions

```csharp
// Product.cs - FULLY GENERATED (can be overwritten)
public class Product : BaseEntity
{
    // Immutable properties, constructor, validation
    private Product() { } // For EF
    
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
    
    public string Name { get; private set; }
    public decimal Price { get; private set; }
}

// ProductDomainService.cs - CUSTOM BUSINESS LOGIC (developer-owned)
public class ProductDomainService : IProductDomainService
{
    public void ActivateProduct(Product product)
    {
        // Custom business logic here
    }
}
```

**Benefits**:
- Simpler mental model - no partial classes
- Regeneration allowed - metadata is source of truth
- Custom logic isolated in services/controllers
- Clear separation: data model vs business logic
- Easier to reason about code ownership

---

## Input Type Hierarchy

The generator uses four key TypeScript types that flow through the system:

| Type | Location | Role |
|---|---|---|
| `DomainModel` | CLI / `IModelProvider` | Top-level JSON input – parsed from file |
| `BoundedContext` | Nested in `DomainModel` | One microservice context (name, namespace, ports, enums, `dataModel`) |
| `DataModel` | Nested in `BoundedContext.dataModel` | Entities and relationships for that context |
| `BoundedContextGenerationContext` | Generator input | Flattened view per context – produced by `DataModelParser.buildGenerationContexts()` |

```typescript
// Top-level JSON shape (what the CLI reads)
interface DomainModel {
  enums?: EnumDefinition[];         // Shared across all contexts
  boundedContexts: BoundedContext[];
}

// One entry in boundedContexts[]
interface BoundedContext {
  name: string;
  namespace: string;
  apiPort?: number;
  dbPort?: number;
  databaseName?: string;
  dtoLayout?: 'class' | 'record';
  ownership?: OwnershipConfig;
  enums?: EnumDefinition[];         // Context-scoped (overrides domain-level on name collision)
  dataModel: DataModel;
}

// Nested inside BoundedContext
interface DataModel {
  entities: Entity[];
  relationships?: Relationship[];
}

// What every generator receives (produced by DataModelParser)
interface BoundedContextGenerationContext {
  boundedContext: BoundedContext;   // Original context metadata
  enums: EnumDefinition[];          // Merged: domain-level + context-level (context wins)
  entities: Entity[];               // Shorthand for boundedContext.dataModel.entities
  relationships: Relationship[];    // Shorthand for boundedContext.dataModel.relationships
}
```

`DataModelParser.buildGenerationContexts(domain)` produces one `BoundedContextGenerationContext` per entry in `domain.boundedContexts`, merging enums and flattening the data model fields.

---

## Architecture

### Component Layers

```
┌─────────────────────────────────────────────┐
│ CLI (cli.ts)                                 │
│ - validate / generate commands               │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Orchestrator                                 │
│ - Coordinates generators                     │
│ - Manages dependencies                       │
│ - Controls execution order                   │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Generators (BaseGenerator)                   │
│ - MetadataGenerator (DataModelMetadata)      │
│ - EntityGenerator                            │
│ - DtoGenerator                               │
│ - ConfigurationGenerator                     │
│ - AbstractionGenerator                       │
│ - ValidatorGenerator                         │
│ - ServiceGenerator                           │
│ - DataAccessGenerator                        │
│ - ControllerGenerator                        │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│ Utilities Layer                              │
│ - NamingConventions (transformations)        │
│ - TypeMapper (JSON → C#)                     │
│ - FileManager (I/O)                          │
│ - DataModelParser (validation)               │
└──────────────────────────────────────────────┘
```

### Data Flow

```
DomainModel JSON (single file, one or many bounded contexts)
    ↓
DataModelParser.validate() (JSON Schema + business rules)
    ↓
DataModelParser.buildGenerationContexts() → BoundedContextGenerationContext[]
    ↓
Orchestrator (loops each BoundedContextGenerationContext)
    ↓ (per bounded context)
Individual Generators receive BoundedContextGenerationContext
    ↓ (for each entity)
TypeMapper + NamingConventions (transformations)
    ↓
Template Context (typed object)
    ↓
Handlebars Template
    ↓
Generated C# Code
    ↓
FileManager (write to disk)
```

---

## Generator Pattern

### Base Generator Structure

All generators extend `BaseGenerator` and implement:

```typescript
export abstract class BaseGenerator {
  protected templates: Map<string, HandlebarsTemplateDelegate>;
  protected templateDir: string;
  
  // Template loading and caching
  protected async loadTemplate(name: string | readonly string[]): Promise<HandlebarsTemplateDelegate>;
  protected async resolveTemplateName(name: string | readonly string[]): Promise<string>;
  
  // Rendering
  protected async renderTemplate(name: string | readonly string[], context: unknown): Promise<string>;
  
  // Write to file
  protected async writeRenderedTemplate(
    templateName: string | readonly string[],
    context: unknown,
    outputPath: string,
    overwrite: boolean
  ): Promise<void>;
  
  // Each generator implements this
  abstract generate(model: BoundedContextGenerationContext): Promise<void>;
}
```

### Generator Implementation Pattern

```typescript
export class EntityGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    // 1. Extract context-specific info
    const contextName = model.boundedContext.name;
    const namespace = model.boundedContext.namespace;
    
    // 2. Determine output paths
    const domainProjectPath = path.join(outputDir, `Inventorization.${contextName}.BL`);
    const entitiesDir = path.join(domainProjectPath, 'Entities');
    
    // 3. Iterate over entities
    for (const entity of model.entities) {
      // 4. Build template context
      const context = this.buildContext(entity, namespace);
      
      // 5. Render and write (overwrite allowed)
      const filePath = path.join(entitiesDir, `${entity.name}.cs`);
      await this.writeRenderedTemplate(
        ['domain/entity/regular.hbs', 'entity.cs.hbs'],
        context,
        filePath,
        true
      );
    }
  }
  
  private buildContext(entity: Entity, namespace: string): TemplateContext {
    // Transform metadata into template-friendly structure
    return {
      namespace,
      entityName: entity.name,
      properties: entity.properties.map(p => this.transformProperty(p)),
      // ... other context
    };
  }
}
```

---

## Template Context Patterns

### Context Structure

Template contexts are **strongly typed** objects that provide data to Handlebars:

```typescript
interface EntityTemplateContext {
  namespace: string;                    // Inventorization.Products
  entityName: string;                   // Product
  description: string;                  // Product in catalog
  auditable: boolean;                   // true/false
  constructorParams: ConstructorParam[];
  validations: string[];                // Validation expressions
  propertyAssignments: string[];        // Property = param;
  properties: PropertyContext[];
  navigationProperties: NavigationContext[];
}
```

### Property Transformation Pattern

```typescript
private transformProperty(property: Property): PropertyContext {
  return {
    name: property.name,                                    // Name
    type: TypeMapper.toCSharpType(property.type, !property.required),  // string
    description: property.description,                      // "Product name"
    defaultValue: property.defaultValue || null,
    validationAttributes: TypeMapper.getValidationAttributes(property)
  };
}
```

### Foreign Key & Navigation Property Pattern

**Critical Rule**: Foreign key properties and navigation properties are **separate** constructs:

#### FK Property Declaration
```json
{
  "name": "CategoryId",
  "type": "Guid",
  "required": true,
  "isForeignKey": true,
  "referencedEntity": "Category",
  "navigationProperty": "Category",
  "description": "Category ID"
}
```

**Generated as regular Guid property**:
```csharp
public Guid CategoryId { get; private set; }
```

#### Navigation Property Generation

Navigation properties are **derived from metadata**, not declared as separate properties:

**Single Navigation Properties** (from FK metadata):
- Generated from `navigationProperty` field in FK property
- Type from `referencedEntity` field
- Always read-only with `null!` initializer

```csharp
// Generated from FK metadata above
public Category Category { get; } = null!;
```

**Collection Navigation Properties** (from relationships):
- Generated from `relationships` array
- OneToMany creates collection on "left" entity

```json
{
  "type": "OneToMany",
  "leftEntity": "Category",
  "rightEntity": "Product",
  "leftNavigationProperty": "Products",
  "rightNavigationProperty": "Category"
}
```

```csharp
// Generated on Category entity
public ICollection<Product> Products { get; } = new List<Product>();
```

#### EntityGenerator Implementation Pattern

```typescript
private buildNavigationProperties(
  properties: Property[],
  entityName: string,
  relationships: Relationship[]
): NavigationContext[] {
  const navProps: NavigationContext[] = [];

  // 1. Single navigation from FK properties
  for (const prop of properties) {
    if (prop.isForeignKey && prop.navigationProperty && prop.referencedEntity) {
      navProps.push({
        name: prop.navigationProperty,           // "Category"
        type: prop.referencedEntity,             // "Category"
        isCollection: false,
        nullable: !prop.required,
        description: `Navigation to ${prop.referencedEntity}`
      });
    }
  }

  // 2. Collection navigation from relationships
  for (const rel of relationships) {
    if (rel.type === 'OneToMany' && 
        rel.leftEntity === entityName && 
        rel.leftNavigationProperty) {
      navProps.push({
        name: rel.leftNavigationProperty,        // "Products"
        type: rel.rightEntity,                   // "Product"
        isCollection: true,
        nullable: false,
        description: `Collection of ${rel.rightEntity}`
      });
    }
  }

  return navProps;
}
```

**Key Points**:
- FK properties **ARE** regular properties (Guid/int type)
- FK properties **ARE** included in constructor parameters
- Navigation properties **ARE NOT** regular properties
- Navigation properties **ARE** derived from FK metadata + relationships
- Collection properties use `ICollection<T>` with `new List<T>()` initializer

### Naming Transformations

Use `NamingConventions` class for all name transformations:

```typescript
// Entity-based transformations
NamingConventions.toCreateDtoName('Product')      // CreateProductDTO
NamingConventions.toDetailsDtoName('Product')     // ProductDetailsDTO
NamingConventions.toDataServiceInterfaceName('Product')  // IProductDataService
NamingConventions.toControllerName('Product')     // ProductsController

// Case transformations
NamingConventions.toCamelCase('ProductName')      // productName
NamingConventions.pluralize('Product')            // Products
```

**Never hardcode naming patterns** - always use `NamingConventions` to ensure consistency.

---

## ADT-Based Search Component Generation

### Overview

The ADT-based search architecture requires 8 infrastructure files per entity (~589 lines total). These are highly repetitive and ideal candidates for code generation. This section documents the generators and templates for ADT search components.

**Background**: See [Architecture.md - ADT-Based Query/Search Architecture](Architecture.md#adt-based-querysearch-architecture) for architectural overview.

**Generated Components** (per entity):

| Component | Lines | Complexity | Template | Customization Need |
|-----------|-------|------------|----------|-------------------|
| QueryBuilder | ~13 | LOW | query-builder.generated.cs.hbs | Rare (ParameterName override) |
| SearchService | ~28 | LOW | search-service.generated.cs.hbs | Rare (custom search logic) |
| QueryController | ~25 | LOW | query-controller.generated.cs.hbs | Medium (custom endpoints) |
| ProjectionMapper | ~250 | **HIGH** | projection-mapper.generated.cs.hbs | Medium (complex nesting) |
| ProjectionMapper Interface | ~3 | LOW | projection-mapper-interface.generated.cs.hbs | Never |
| Projection DTO | ~30 | LOW | projection-dto.generated.cs.hbs | Rare (computed properties) |
| SearchFields | ~40 | LOW | search-fields.generated.cs.hbs | Never |
| SearchQueryValidator | ~200 | **HIGH** | search-query-validator.generated.cs.hbs | Rare (custom validation) |

**Total: ~589 lines per entity** (ProjectionMapper and Validator are most complex)

### Generator Classes

All generators located in `generation/code/src/generators/`

#### QueryBuilderGenerator

**Purpose**: Generate QueryBuilder class inheriting from `BaseQueryBuilder<TEntity>` (non-owned) or `BaseQueryBuilder<TEntity, TOwnership>` (owned)

**Output**: `BL/DataAccess/{Entity}QueryBuilder.cs`

**Template Context**:
```typescript
interface QueryBuilderContext {
  namespace: string;              // Inventorization.Goods
  entityName: string;             // Good
  isOwned: boolean;               // true → inherits BaseQueryBuilder<Good, UserTenantOwnership>
  ownershipValueObject: string;   // "UserTenantOwnership" (only present when isOwned=true)
  generationStamp: string;        // 20260214-a3f8c891
  generatedAt: string;            // 2026-02-14 12:34:56 UTC
  sourceFile: string;             // goods-context.json
}
```

**Implementation Pattern**:
```typescript
export class QueryBuilderGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const contextName = model.boundedContext.name;
    const namespace = `Inventorization.${contextName}`;
    const domainProjectPath = path.join(this.outputDir, `${namespace}.Domain`);
    const dataAccessDir = path.join(domainProjectPath, 'DataAccess');
    
    for (const entity of model.entities) {
      const context = {
        namespace,
        entityName: entity.name,
        generationStamp: this.generationStamp,
        generatedAt: new Date().toISOString(),
        sourceFile: model.boundedContext.name
      };
      
      const filePath = path.join(dataAccessDir, `${entity.name}QueryBuilder.cs`);
      await this.writeRenderedTemplate(
        'query-builder.generated.cs.hbs',
        context,
        filePath,
        true  // Overwrite allowed
      );
    }
  }
}
```

**Why Generate?** Even though it's small, ensures consistency and saves ~13 lines × number of entities

#### SearchServiceGenerator

**Purpose**: Generate SearchService inheriting from `BaseSearchService<TEntity, TProjection>` (non-owned) or `BaseSearchService<TEntity, TProjection, TOwnership>` (owned)

**Output**: `BL/Services/{Entity}SearchService.cs`

**Template Context**:
```typescript
interface SearchServiceContext {
  namespace: string;
  entityName: string;
  projectionName: string;         // GoodProjection
  isOwned: boolean;               // true → adds ICurrentIdentityContext ctor param
  ownershipValueObject: string;   // "UserTenantOwnership" (only present when isOwned=true)
  generationStamp: string;
  generatedAt: string;
  sourceFile: string;
}
```

**Generated Code Structure (non-owned)**:
```csharp
public class GoodSearchService : BaseSearchService<Good, GoodProjection>
{
    public GoodSearchService(
        IRepository<Good> repository,
        IQueryBuilder<Good> queryBuilder,
        IProjectionMapper<Good, GoodProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ILogger<GoodSearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, logger) { }
}
```

**Generated Code Structure (owned)**:
```csharp
public class OrderSearchService : BaseSearchService<Order, OrderProjection, UserTenantOwnership>
{
    public OrderSearchService(
        IRepository<Order> repository,
        IQueryBuilder<Order, UserTenantOwnership> queryBuilder,
        IProjectionMapper<Order, OrderProjection> projectionMapper,
        ProjectionExpressionBuilder expressionBuilder,
        IValidator<SearchQuery> validator,
        ICurrentIdentityContext<UserTenantOwnership> identityContext,
        ILogger<OrderSearchService> logger)
        : base(repository, queryBuilder, projectionMapper, expressionBuilder, validator, identityContext, logger) { }
}
```

#### QueryControllerGenerator

**Purpose**: Generate Query controller with HTTP endpoints

**Output**: `API/Controllers/{Entity}sQueryController.cs` (~25 lines)

**Template Context**:
```typescript
interface QueryControllerContext {
  namespace: string;
  entityName: string;
  entityNamePlural: string;       // Goods (for route)
  projectionName: string;
  routePrefix: string;            // api/goods/query
  generationStamp: string;
  generatedAt: string;
  sourceFile: string;
}
```

**Route Convention**: `api/{entityPlural}/query` (e.g., `api/goods/query`, `api/categories/query`)

**Endpoints Generated**:
- `POST /api/{entityPlural}/query` → Regular search
- `POST /api/{entityPlural}/query/transform` → Field transformations

#### ProjectionMapperGenerator

**Purpose**: Generate ProjectionMapper implementing 4 abstract methods from ProjectionMapperBase

**Output**: `BL/Mappers/Projection/{Entity}ProjectionMapper.cs` (~250 lines)

**Template Context** (most complex):
```typescript
interface ProjectionMapperContext {
  namespace: string;
  entityName: string;
  projectionName: string;
  properties: PropertyContext[];        // Direct entity properties
  relationships: RelationshipContext[]; // Navigation properties
  maxDefaultDepth: number;              // Default depth for AllDeep (e.g., 3)
  generationStamp: string;
  generatedAt: string;
  sourceFile: string;
}

interface PropertyContext {
  name: string;                   // Name
  type: string;                   // string
  isNullable: boolean;           // false
  camelName: string;             // name
}

interface RelationshipContext {
  name: string;                   // Category
  targetEntity: string;           // Category
  targetProjection: string;       // CategoryProjection
  targetMapperInterface: string;  // ICategoryProjectionMapper
  isCollection: boolean;          // false
  nullable: boolean;              // true
  camelName: string;              // category
}
```

**Generated Methods**:

1. **GetAllFieldsProjection** - Expression tree for EF Core:
```csharp
protected override Expression<Func<Good, GoodProjection>> GetAllFieldsProjection(bool deep, int depth)
{
    return g => new GoodProjection
    {
        Id = g.Id,
        Name = g.Name,
        // ... all scalar properties
        
        // Conditional nested projection
        Category = deep && depth > 0 && g.Category != null 
            ? new CategoryProjection 
            { 
                Id = g.Category.Id,
                Name = g.Category.Name,
                // Recurse if still have depth
                ParentCategory = depth > 1 && g.Category.ParentCategory != null
                    ? new CategoryProjection { Id = g.Category.ParentCategory.Id, ... }
                    : null
            } 
            : null
    };
}
```

2. **BuildSelectiveProjection** - HashSet pattern for selective fields:
```csharp
protected override Expression<Func<Good, GoodProjection>> BuildSelectiveProjection(
    ProjectionRequest projection)
{
    // Evaluate OUTSIDE expression tree (EF Core incompatible operations)
    var requestedFields = new HashSet<string>(
        projection.Fields.Select(f => f.FieldName), 
        StringComparer.OrdinalIgnoreCase);
    
    var hasName = requestedFields.Contains("Name");
    var hasPrice = requestedFields.Contains("Price");
    var hasCategoryName = requestedFields.Contains("Category.Name");
    
    // Use boolean constants in expression tree
    return g => new GoodProjection
    {
        Name = hasName ? g.Name : null,
        Price = hasPrice ? g.Price : null,
        Category = hasCategoryName ? new CategoryProjection { Name = g.Category.Name } : null
    };
}
```

3. **MapAllFields** - In-memory mapping with depth control:
```csharp
protected override void MapAllFields(Good entity, GoodProjection result, 
    bool deep, int maxDepth, int currentDepth)
{
    // Map all scalar properties
    result.Id = entity.Id;
    result.Name = entity.Name;
    result.Price = entity.Price;
    // ... all properties
    
    // Conditionally map relationships with depth tracking
    if (deep && currentDepth < maxDepth && entity.Category != null)
    {
        var categoryProjection = ProjectionRequest.AllDeep(maxDepth - currentDepth - 1);
        result.Category = _categoryMapper.Map(
            entity.Category, 
            categoryProjection, 
            currentDepth + 1  // Increment depth
        );
    }
}
```

4. **MapField** - Switch statement for individual field mapping:
```csharp
protected override void MapField(Good entity, GoodProjection result, string fieldName, 
    int maxDepth, int currentDepth)
{
    switch (fieldName.ToLower())
    {
        case "id": result.Id = entity.Id; break;
        case "name": result.Name = entity.Name; break;
        case "price": result.Price = entity.Price; break;
        
        // Nested field syntax: "Category.Name"
        case "category.name":
            if (entity.Category != null)
            {
                result.Category ??= new CategoryProjection();
                result.Category.Name = entity.Category.Name;
            }
            break;
    }
}
```

**Generation Challenges**:
- **Depth Recursion**: Template must generate nested ternary operators for depth levels
- **HashSet Pattern**: Template evaluates field checks outside expression tree
- **Related Entity Mappers**: Must inject mapper interfaces for each relationship
- **Null Safety**: All relationship accesses must be null-checked

**Template Complexity**: This is the most complex generator due to expression tree requirements

#### ProjectionMapperInterfaceGenerator

**Purpose**: Generate marker interface extending IProjectionMapper

**Output**: `BL/Mappers/Projection/I{Entity}ProjectionMapper.cs` (~3 lines)

**Template Context**:
```typescript
interface ProjectionMapperInterfaceContext {
  namespace: string;
  entityName: string;
  projectionName: string;
  generationStamp: string;
}
```

**Generated Code**:
```csharp
public interface IGoodProjectionMapper : IProjectionMapper<Good, GoodProjection> { }
```

**Purpose**: Enables type-safe dependency injection for nested mappers

#### ProjectionDtoGenerator

**Purpose**: Generate Projection DTO with nullable properties

**Output**: `DTO/ADTs/{Entity}Projection.cs` (~30 lines)

**Template Context**:
```typescript
interface ProjectionDtoContext {
  namespace: string;
  entityName: string;
  properties: PropertyContext[];        // All nullable
  relationships: RelationshipContext[]; // All nullable projections
  generationStamp: string;
}
```

**Generated Code**:
```csharp
public class GoodProjection
{
    [JsonPropertyName("id")]
    public Guid? Id { get; init; }
    
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    
    [JsonPropertyName("price")]
    public decimal? Price { get; init; }
    
    [JsonPropertyName("category")]
    public CategoryProjection? Category { get; init; }
}
```

**Key Points**:
- All properties nullable (selective projections may omit fields)
- JsonPropertyName for camelCase JSON
- Record or class with init-only setters
- Related entities as nested projection types

#### SearchFieldsGenerator

**Purpose**: Generate field name constants for type-safe queries

**Output**: `DTO/ADTs/{Entity}SearchFields.cs` (~40 lines)

**Template Context**:
```typescript
interface SearchFieldsContext {
  namespace: string;
  entityName: string;
  properties: PropertyContext[];
  relationships: RelationshipContext[];
  generationStamp: string;
}
```

**Generated Code**:
```csharp
public static class GoodSearchFields
{
    // Direct properties
    public const string Id = "Id";
    public const string Name = "Name";
    public const string Price = "Price";
    public const string CategoryId = "CategoryId";
    
    // Nested properties (dot notation)
    public const string CategoryName = "Category.Name";
    public const string CategoryParentCategoryName = "Category.ParentCategory.Name";
    
    // Validation helper
    public static bool IsValidField(string fieldName)
    {
        return fieldName switch
        {
            Id or Name or Price or CategoryId or CategoryName => true,
            _ => false
        };
    }
}
```

**Benefits**:
- Compile-time safety for field names
- IntelliSense support in client code
- Prevents typos in field references

#### SearchQueryValidatorGenerator

**Purpose**: Generate validator using DataModelMetadata

**Output**: `BL/Validators/{Entity}SearchQueryValidator.cs` (~200 lines)

**Template Context**:
```typescript
interface SearchQueryValidatorContext {
  namespace: string;
  entityName: string;
  metadataClassName: string;  // DataModelMetadata.Good
  properties: PropertyValidationContext[];
  generationStamp: string;
}

interface PropertyValidationContext {
  name: string;
  type: string;
  isNullable: boolean;
  supportedOperators: FilterOperator[];  // Based on type
}
```

**Generated Code**:
```csharp
public class GoodSearchQueryValidator : IValidator<SearchQuery>
{
    private static readonly IDataModelMetadata<Good> Metadata = DataModelMetadata.Good;
    
    public async Task<ValidationResult> ValidateAsync(
        SearchQuery query, 
        CancellationToken ct = default)
    {
        var errors = new List<string>();
        
        if (query.Filter != null)
            ValidateFilter(query.Filter, errors);
        
        if (query.Projection != null)
            ValidateProjection(query.Projection, errors);
        
        if (query.Sort != null)
ValidateSort(query.Sort, errors);
        
        return errors.Any() 
            ? ValidationResult.Invalid(errors)
            : ValidationResult.Valid();
    }
    
    private void ValidateFilter(FilterExpression filter, List<string> errors)
    {
        switch (filter)
        {
            case LeafFilter leaf:
                ValidateFilterCondition(leaf.Condition, errors);
                break;
            case AndFilter and:
                foreach (var f in and.Filters) ValidateFilter(f, errors);
                break;
            case OrFilter or:
                foreach (var f in or.Filters) ValidateFilter(f, errors);
                break;
        }
    }
    
    private void ValidateFilterCondition(FilterCondition condition, List<string> errors)
    {
        // Check field exists
        if (!Metadata.Properties.ContainsKey(condition.FieldName))
        {
            errors.Add($"Field '{condition.FieldName}' does not exist on entity Good");
            return;
        }
        
        var propertyMetadata = Metadata.Properties[condition.FieldName];
        
        // Validate type compatibility
        if (condition.Value != null)
        {
            var expectedType = propertyMetadata.Type;
            var actualType = condition.Value.GetType();
            
            if (!IsTypeCompatible(expectedType, actualType, propertyMetadata.IsNullable))
            {
                errors.Add($"Field '{condition.FieldName}' expects type {expectedType.Name} but got {actualType.Name}");
            }
        }
        
        // Validate operator compatibility
        ValidateOperator(condition, propertyMetadata, errors);
    }
    
    private void ValidateOperator(FilterCondition condition, PropertyMetadata property, List<string> errors)
    {
        switch (condition.Operator)
        {
            case FilterOperator.Contains:
            case FilterOperator.StartsWith:
            case FilterOperator.EndsWith:
                if (property.Type != typeof(string))
                    errors.Add($"String operators only valid for string fields, but '{condition.FieldName}' is {property.Type.Name}");
                break;
                
            case FilterOperator.GreaterThan:
            case FilterOperator.LessThan:
                if (!IsComparable(property.Type))
                    errors.Add($"Comparison operators require comparable types, but '{condition.FieldName}' is {property.Type.Name}");
                break;
        }
    }
}
```

**Validation Types**:
1. **Field Existence**: Field must exist in metadata
2. **Type Compatibility**: Value type must match property type
3. **Operator Compatibility**: Operator must be valid for property type
4. **Relationship Depth**: Validate nested field paths (e.g., "Category.Name")

**Metadata Usage**: Generator references `DataModelMetadata.{Entity}` for runtime validation

### Template Patterns

#### Depth-Controlled Nesting Template

For generating nested projections with depth limits:

```handlebars
{{#if deep}}
{{entityName}} = deep && depth > 0 && entity.{{entityName}} != null 
    ? new {{projectionName}}
    {
        {{#each properties}}
        {{name}} = entity.{{../entityName}}.{{name}},
        {{/each}}
        
        {{#each nestedRelationships}}
        {{name}} = depth > 1 && entity.{{../entityName}}.{{name}} != null
            ? new {{projectionName}} { /* recursive nesting */ }
            : null,
        {{/each}}
    }
    : null
{{/if}}
```

#### HashSet Field Evaluation Template

For selective field mapping:

```handlebars
// Evaluate outside expression tree
var requestedFields = new HashSet<string>(
    projection.Fields.Select(f => f.FieldName), 
    StringComparer.OrdinalIgnoreCase);

{{#each properties}}
var has{{pascalCase name}} = requestedFields.Contains("{{name}}");
{{/each}}

// Use in expression tree
return entity => new {{projectionName}}
{
    {{#each properties}}
    {{name}} = has{{pascalCase name}} ? entity.{{name}} : null,
    {{/each}}
};
```

#### Relationship Mapper Injection Template

```handlebars
{{#each relationships}}
private readonly I{{targetEntity}}ProjectionMapper _{{camelCase targetEntity}}Mapper;
{{/each}}

public {{entityName}}ProjectionMapper(
    {{#each relationships}}
    I{{targetEntity}}ProjectionMapper {{camelCase targetEntity}}Mapper{{#unless @last}},{{/unless}}
    {{/each}}
)
{
    {{#each relationships}}
    _{{camelCase targetEntity}}Mapper = {{camelCase targetEntity}}Mapper;
    {{/each}}
}
```

### DI Registration Code Generation

Generate registration code for each entity:

```csharp
// Query Builder
builder.Services.AddScoped<IQueryBuilder<{{entityName}}>, {{entityName}}QueryBuilder>();

// Projection Mapper (dual registration)
builder.Services.AddScoped<I{{entityName}}ProjectionMapper, {{entityName}}ProjectionMapper>();
builder.Services.AddScoped<IProjectionMapper<{{entityName}}, {{entityName}}Projection>>(
    sp => sp.GetRequiredService<I{{entityName}}ProjectionMapper>());

// Validator
builder.Services.AddScoped<IValidator<SearchQuery>, {{entityName}}SearchQueryValidator>();

// Search Service (dual registration)
builder.Services.AddScoped<{{entityName}}SearchService>();
builder.Services.AddScoped<ISearchService<{{entityName}}, {{entityName}}Projection>>(
    sp => sp.GetRequiredService<{{entityName}}SearchService>());
```

**Note**: ProjectionExpressionBuilder registered once as shared service

### Generation Phases

ADT search components added to existing generation phases:

```typescript
// Phase 5: Domain Infrastructure (after entities, before services)
phases.push({
  name: "ADT Query Infrastructure",
  generators: [
    new QueryBuilderGenerator(),
    new ProjectionMapperInterfaceGenerator(),
    new ProjectionMapperGenerator(),
    new SearchFieldsGenerator(),
    new ProjectionDtoGenerator(),
  ]
});

// Phase 6: Domain Services
phases.push({
  name: "Domain Services",
  generators: [
    // ... existing service generators
    new SearchServiceGenerator(),
    new SearchQueryValidatorGenerator(),
  ]
});

// Phase 7: API Layer
phases.push({
  name: "API Controllers",
  generators: [
    // ... existing controller generators
    new QueryControllerGenerator(),
  ]
});
```

**Dependencies**:
- Requires: MetadataGenerator (DataModelMetadata), EntityGenerator
- Before: DI registration, tests

### Best Practices

1. **Overwrite Policy**: All ADT search components use `overwrite: true` - regeneration expected
2. **Custom Logic**: Business rules go in domain services, not in generated SearchService
3. **Depth Defaults**: Use depth=3 as default for AllDeep projections (balance between completeness and performance)
4. **Null Safety**: All templates must handle nullable relationships
5. **HashSet Pattern**: Always evaluate field checks outside expression trees for EF Core compatibility
6. **Metadata Sync**: Validators must stay in sync with DataModelMetadata
7. **Testing**: Generate unit tests for validators and projection mappers

### Customization Points

**When to override generated code**:

1. **QueryBuilder**: Override `ParameterName` if conflict with other variables
2. **SearchService**: Override search methods for custom filtering/authorization
3. **QueryController**: Add custom endpoints for specialized queries
4. **ProjectionMapper**: Override for complex computed properties or custom nesting logic

**When NOT to modify**:
- ProjectionMapper Interface (always marker interface)
- SearchFields (always simple constants)
- Projection DTO (pure data structure)

---

## Handlebars Template Patterns

### Template Structure

```handlebars
{{!-- Header comment --}}
// <auto-generated>
//     This code was generated by the BoundedContext Code Generator.
//     Generation Stamp: {{generationStamp}}
//     Generated: {{generatedAt}}
//     Source: {{sourceFile}}
//
//     Changes to this file will be lost when regenerated.
//     Custom business logic should be added to separate domain services.
// </auto-generated>

{{!-- Using statements --}}
using {{namespace}}.Base;
{{#if isOwned}}
using Inventorization.Base.Ownership;
{{/if}}

{{!-- Namespace and class --}}
namespace {{namespace}}.Domain.Entities;

/// <summary>
/// {{description}}
/// </summary>
public class {{entityName}} : {{baseEntityClass}}
{
    {{!-- Constructor --}}
    public {{entityName}}(
        {{#each constructorParams}}
        {{type}} {{paramName}}{{#unless @last}},{{/unless}}
        {{/each}}
    )
    {
        {{#each validations}}
        {{this}}
        {{/each}}
        
        {{#each propertyAssignments}}
        {{this}}
        {{/each}}
    }
    
    {{!-- Properties --}}
    {{#each properties}}
    public {{type}} {{name}} { get; private set; }
    {{/each}}
}
```

`{{baseEntityClass}}` resolves to `BaseEntity` (non-owned) or `OwnedBaseEntity<{{ownershipValueObject}}>` (owned). See [Ownership Support in Data Models](#ownership-support-in-data-models).

### Custom Handlebars Helpers

```typescript
// Conditional rendering
Handlebars.registerHelper('ifEquals', (arg1, arg2, options) => {
  return arg1 === arg2 ? options.fn(this) : options.inverse(this);
});

// Logical operations
Handlebars.registerHelper('or', (...args) => {
  const options = args[args.length - 1] as Handlebars.HelperOptions;
  const values = args.slice(0, -1);
  return values.some((v) => !!v) ? options.fn(this) : options.inverse(this);
});

// String transformations
Handlebars.registerHelper('camelCase', (str: string) => {
  return str.charAt(0).toLowerCase() + str.slice(1);
});
```

### Template Best Practices

1. **Always include auto-generated header** - warns developers about regeneration
2. **Include generation metadata** - timestamp, source file for traceability
3. **Preserve whitespace carefully** - C# is whitespace-sensitive for readability
4. **Use XML comments** - `/// <summary>` for all public types/members
5. **Keep logic in TypeScript** - templates should be dumb; prepare data in generator
6. **Test with edge cases** - empty collections, optional properties, etc.
7. **EF Core compatibility** - include parameterless constructor for entities
8. **Use triple braces for code** - `{{{this}}}` for raw output, `{{this}}` for text

### HTML Encoding: Double vs Triple Braces

**Critical Rule**: Use triple braces `{{{...}}}` for outputting **pre-generated code strings**.

```handlebars
{{!-- WRONG - will HTML-encode special characters --}}
{{#each validations}}
{{this}}  {{!-- Produces: if (name.Length &gt; 100) --}}
{{/each}}

{{!-- CORRECT - outputs raw code --}}
{{#each validations}}
{{{this}}}  {{!-- Produces: if (name.Length > 100) --}}
{{/each}}
```

**When to use `{{...}}`** (double braces - HTML-encoded):
- Property names, descriptions
- Namespace names, entity names
- Any text that's NOT pre-generated code

**When to use `{{{...}}}`** (triple braces - raw output):
- `validations` array (pre-generated if statements)
- `propertyAssignments` array (pre-generated assignment code)
- `validationAttributes` array (pre-generated C# attributes)
- Any pre-built code snippets from TypeScript

```handlebars
{{!-- Text values - use double braces --}}
namespace {{namespace}}.Domain.Entities;  ✅
public {{type}} {{name}} { get; set; }    ✅

{{!-- Pre-generated code - use triple braces --}}
{{#each validations}}
{{{this}}}  ✅
{{/each}}

{{#each validationAttributes}}
{{{this}}}  ✅
{{/each}}
```

---

## Meta Project & DataModelMetadata

### Purpose

The **Meta Project** (`Inventorization.{Context}.Meta`) serves as the centralized metadata repository for a bounded context. It contains two core generated classes:

1. **DataModelMetadata.cs** - Entity structure, property metadata, validation rules
2. **DataModelRelationships.cs** - Relationship definitions between entities

### Why a Separate Project?

**Dependency Flow**:
```
Meta ← Domain, DTO, API, API.Tests
```

Meta project has **zero dependencies** on other projects, making metadata reusable across:
- Domain (for entity validation)
- DTO (for mapping validation)
- API (for endpoint documentation)
- Frontend (for form generation, validation)

### DataModelMetadata Structure

Generated static class containing metadata for all entities:

```csharp
public static class DataModelMetadata
{
    public static EntityMetadata Product { get; } = new EntityMetadata.Builder()
        .WithName("Product")
        .WithTable("Products", "catalog")
        .WithProperty(prop => prop
            .WithName("Name")
            .WithType("string", false)
            .WithValidation(val => val
                .Required()
                .MaxLength(200)))
        .WithProperty(prop => prop
            .WithName("Price")
            .WithType("decimal", false)
            .WithValidation(val => val
                .Required()
                .Precision(18, 2)))
        .WithIndex(idx => idx
            .Name("IX_Products_SKU")
            .Columns("SKU")
            .IsUnique(true))
        .Build();
    
    public static EntityMetadata Category { get; } = ...;
}
```

**Usage in Validators**:
```csharp
public class ProductValidator : IValidator<CreateProductDTO>
{
    public ValidationResult Validate(CreateProductDTO dto)
    {
        var metadata = DataModelMetadata.Product;
        var nameProperty = metadata.Properties.First(p => p.Name == "Name");
        
        if (dto.Name?.Length > nameProperty.Validation.MaxLength)
            return ValidationResult.Failure($"Name exceeds {nameProperty.Validation.MaxLength} characters");
            
        return ValidationResult.Success;
    }
}
```

**Usage in EF Configurations**:
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        var metadata = DataModelMetadata.Product;
        builder.ToTable(metadata.TableName, metadata.SchemaName);
        
        foreach (var property in metadata.Properties)
        {
            // Apply configurations from metadata
        }
    }
}
```

### DataModelRelationships Structure

Generated static class containing all relationship metadata:

```csharp
public static class DataModelRelationships
{
    public static RelationshipMetadata CategoryProducts { get; } = new RelationshipMetadata.Builder()
        .WithName("CategoryProducts")
        .WithType(RelationshipType.OneToMany)
        .WithPrincipal("Category", "CategoryId")
        .WithDependent("Product", "CategoryId")
        .WithCardinality(Cardinality.Required)
        .WithDeleteBehavior(DeleteBehavior.Restrict)
        .Build();
    
    public static RelationshipMetadata ProductTags { get; } = ...;
}
```

**Usage in EF Configurations**:
```csharp
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        var relationship = DataModelRelationships.CategoryProducts;
        
        builder.HasMany(relationship.DependentNavigationProperty)
               .WithOne(relationship.PrincipalNavigationProperty)
               .HasForeignKey(relationship.ForeignKeyProperty)
               .OnDelete(relationship.DeleteBehavior);
    }
}
```

### Metadata Benefits

1. **Single Source of Truth**: All validation rules, indexes, relationships defined once
2. **Type Safety**: Strongly-typed metadata accessible via IntelliSense
3. **Reusability**: Same metadata drives Domain, DTO, API, Frontend
4. **Discoverability**: IntelliSense shows all available entity metadata
5. **Maintainability**: Changes to metadata auto-propagate to consumers
6. **Documentation**: Self-documenting - metadata explains entity structure

### Generation Context

The `MetadataGenerator` builds metadata from JSON data model:

```typescript
buildEntityMetadataContext(entity: Entity): EntityMetadataContext {
  return {
    entityName: entity.name,
    tableName: entity.tableName || `${entity.name}s`,
    schemaName: entity.schemaName || 'dbo',
    properties: entity.properties.map(p => ({
      name: p.name,
      type: TypeMapper.toCSharpType(p.type, p.nullable),
      isNullable: p.nullable,
      validation: {
        isRequired: !p.nullable,
        maxLength: p.validations?.maxLength,
        precision: p.validations?.precision,
        scale: p.validations?.scale,
        regex: p.validations?.regex,
        // ... other validation rules
      }
    })),
    indexes: entity.indexes || [],
    uniqueConstraints: entity.uniqueConstraints || []
  };
}
```

---

## Type Safety Patterns

### No `any` Types

**Rule**: The `@typescript-eslint/no-explicit-any` rule is set to `error`.

**Instead of `any`, use**:

```typescript
// For truly unknown values
function parse(data: unknown): DataModel {
  // Type guard required before use
}

// For Handlebars helpers
Handlebars.registerHelper('helper', (...args: unknown[]) => {
  const options = args[args.length - 1] as Handlebars.HelperOptions;
});

// For dictionaries
const paths: Record<string, string> = { dto: '...', domain: '...' };

// For arrays of objects
const properties: Array<{ name: string; type: string }> = [...];
```

### Type Mapping

Use `TypeMapper` for all JSON → C# type conversions:

```typescript
TypeMapper.toCSharpType('string', false)           // string
TypeMapper.toCSharpType('int', true)               // int?
TypeMapper.toCSharpType('decimal', false)          // decimal

TypeMapper.getValidationAttributes(property)       // [Required], [StringLength(200)]
TypeMapper.isNumericType('decimal')                // true
TypeMapper.isStringType('string')                  // true
```

---

## Generation Phases

### Dependency Order

Generators must run in dependency order to avoid missing references:

```
Phase 1: Metadata (DataModelMetadata, DataModelRelationships)
   ↓
Phase 2: Common Project (Enums)
   ↓
Phase 3: Entities (Domain Models)
   ↓
Phase 4: DTOs (Data Transfer Objects)
   ↓
Phase 5: EntityConfigurations (EF Core mappings)
   ↓
Phase 6: DbContext + UnitOfWork
   ↓
Phase 7: Abstractions + ADT Query Infrastructure
   ↓
Phase 8: Validators
   ↓
Phase 9: DataServices
   ↓
Phase 10: Controllers
   ↓
Phase 11: DI + API Program
   ↓
Phase 12: Tests (optional via `skipTests`)
  ↓
Phase 13: Project files
```

### Orchestrator Pattern

```typescript
export class Orchestrator {
  async generate(domain: DomainModel): Promise<void> {
    // Build a per-bounded-context flattened view (merges domain + context enums)
    const contexts = this.parser.buildGenerationContexts(domain);

    // contextScheduler controls how many bounded contexts run simultaneously
    await this.contextScheduler.run(
      contexts.map((ctx) => async () => this.generateBoundedContext(ctx))
    );
  }

  private async generateBoundedContext(model: BoundedContextGenerationContext): Promise<void> {
    const enabledSlots = new Set<string>(['core']);
    enabledSlots.add(this.resolvePresentationKind());
    enabledSlots.add(this.resolveDataLayerKind());
    if (!this.options.skipTests) enabledSlots.add('tests');

    // Resolve validated execution plan (phase + dependency order)
    const executionPlan = this.registry.resolveExecutionPlan(model, context, enabledSlots);

    // Group by phase — phases must run sequentially; tasks within a phase are independent
    const phaseGroups = Orchestrator.groupByPhase(executionPlan);
    for (const group of phaseGroups) {
      // generatorScheduler controls how many generators within a phase run simultaneously
      await this.generatorScheduler.run(
        group.map((r) => () => r.generator.generate(model, context))
      );
    }
  }
}
```

---

## Execution Scheduling

### Isolation by Design

All concurrency knowledge is isolated in two classes behind the `IExecutionScheduler` interface:

```typescript
// src/abstractions/IExecutionScheduler.ts
export interface IExecutionScheduler {
  /**
   * Run all tasks with the configured concurrency policy.
   * Resolves when every task completes.
   */
  run(tasks: ReadonlyArray<() => Promise<void>>): Promise<void>;
}
```

The `Orchestrator`, generators, and all application logic depend **only** on this interface — they never import `p-limit` or any concurrency primitive directly.

### Implementations

| Class | Behaviour | Default? |
|---|---|---|
| `SequentialScheduler` | Runs tasks one at a time (`for await` loop) | ✅ Yes |
| `ConcurrentScheduler(n)` | Runs up to `n` tasks simultaneously via `p-limit` | No — opt-in via `--concurrency` |

### Two Independent Parallelism Levels

The `Orchestrator` holds two scheduler instances:

| Field | Controls |
|---|---|
| `contextScheduler` | Outer loop — how many **bounded contexts** generate simultaneously |
| `generatorScheduler` | Inner loop — how many **generators within a single phase** run simultaneously |

Phase groups always remain sequential (phase 1 completes before phase 2 begins). Only tasks inside the same phase batch are eligible for concurrency because they write to non-overlapping output directories.

### Why Same-Phase Generators Are Safe to Parallelize

Every generator writes exclusively to its own project subdirectory:

| Generator | Output path |
|---|---|
| `EntityGenerator` | `BC.BL/Entities/` |
| `ConfigurationGenerator` | `BC.BL/EntityConfigurations/` |
| `DtoGenerator` | `BC.DTO/DTO/{E}/` |
| `ControllerGenerator` | `BC.API/Controllers/` |

No two generators share an output file, so concurrent writes within a phase are race-condition-free.

### CLI Usage

```bash
# Sequential (default) — preserves existing behaviour
node dist/cli.js generate data.json --output-dir ../backend

# Concurrent — up to 4 tasks in parallel at each level
node dist/cli.js generate data.json --output-dir ../backend --concurrency 4
```

### Wiring in the Composition Root

```typescript
// cli.ts  (composition root)
const scheduler = options.concurrency !== undefined
  ? new ConcurrentScheduler(options.concurrency)
  : new SequentialScheduler();

const orchestrator = new Orchestrator(resultWriter, {
  // ...
  contextScheduler: scheduler,
  generatorScheduler: scheduler,
});
```

A single scheduler instance is shared across both levels. If different limits are ever required (for example, allow 8 concurrent generators but only 2 contexts), the `OrchestratorOptions` accepts two independent `IExecutionScheduler` values so the calling site can pass different implementations.

---

## Blueprint Schema (Comprehensive)

Blueprint defines architecture/layout strategy independently from `data-model` business shape.

### CLI Integration

- `generate <data-model> --blueprint <path>`
- `validate <data-model> --blueprint <path>`

### Ownership & Conflict Policy

- Blueprint is authoritative for architecture slots (`presentation`, `dataService.dataAccess`, `dataService.dto`).
- If `data-model` sets `boundedContext.dtoLayout` and blueprint sets different DTO style, generation fails (fail-on-conflict).

---

## Ownership Support in Data Models

The generator supports the ownership system (see [Architecture.md — Ownership System](Architecture.md#ownership-system)) via two fields:

### `boundedContext.ownership` (context-level)

Configures the ownership value object and factory for the entire bounded context:

```json
{
  "boundedContexts": [
    {
      "name": "Commerce",
      "namespace": "Inventorization.Commerce",
      "ownership": {
        "enabled": true,
        "valueObject": "UserTenantOwnership",
        "factory": "UserTenantOwnershipFactory"
      },
      "dataModel": {
        "entities": []
      }
    }
  ]
}
```

| Field | Type | Default | Notes |
|---|---|---|---|
| `enabled` | `boolean` | `false` | Activates ownership DI registration and owned-entity scaffolding |
| `valueObject` | `string` | `"UserTenantOwnership"` | Must be a class in `Inventorization.Base.Ownership` or registered custom VO |
| `factory` | `string` | `"UserTenantOwnershipFactory"` | Must implement `IOwnershipFactory<TOwnership>` |

When `ownership.enabled = true` the generator:
1. Emits `services.AddOwnershipServices<TOwnership, TFactory>()` in the DI extension
2. Adds `using Inventorization.Base.AspNetCore.Extensions` to the DI file

### `entity.owned` (entity-level)

Marks a specific entity as owned by a user/tenant:

```json
{
  "boundedContexts": [
    {
      "name": "Commerce",
      "namespace": "Inventorization.Commerce",
      "dataModel": {
        "entities": [
          { "name": "Category", "tableName": "Categories" },
          { "name": "Order",    "tableName": "Orders", "owned": true }
        ]
      }
    }
  ]
}
```

When `owned: true` the generator produces:

| Component | Non-owned output | Owned output |
|---|---|---|
| Entity base class | `BaseEntity` | `OwnedBaseEntity<UserTenantOwnership>` |
| QueryBuilder | `BaseQueryBuilder<Order>` | `BaseQueryBuilder<Order, UserTenantOwnership>` (+ `ICurrentIdentityContext` ctor param) |
| SearchService | `BaseSearchService<Order, OrderProjection>` | `BaseSearchService<Order, OrderProjection, UserTenantOwnership>` |
| DataService constructor | `(unitOfWork, repo, logger)` | `(unitOfWork, repo, identityContext, logger)` |
| CRUD controller | No `ICurrentUserService` | Injects `ICurrentUserService<TOwnership>`, overrides `UpdateAsync`/`DeleteAsync` with access check |
| DI registration | `IQueryBuilder<Order>` | `IQueryBuilder<Order, UserTenantOwnership>` + forwarding `IQueryBuilder<Order>` |

**Constraint**: `entity.owned: true` requires `boundedContext.ownership.enabled: true`. The generator emits a warning and falls back to non-owned if the context ownership is not configured.

### Blueprint Shape (v1)

```json
{
  "version": "1",
  "boundedContext": {
    "common": { "enums": "SmartEnums" },
    "presentation": { "kind": "controllers" },
    "dataService": {
      "dto": "class",
      "uow": "injected",
      "dataAccess": {
        "orm": {
          "kind": "ef-core",
          "provider": "npgsql"
        },
        "entities": "immutable"
      },
      "domain": "default"
    }
  }
}
```

### Supported Values

- `presentation.kind`: `controllers | minimal-api | grpc`
  - `grpc` is schema-valid but currently fail-fast in orchestrator.
- `dataService.dto`: `class | record`
- `dataService.uow`: `injected`
- `dataService.dataAccess.orm.kind`: `ef-core`
- `dataService.dataAccess.orm.provider`: `npgsql`
- `dataService.dataAccess.ado.provider`: `npgsql`
- `dataService.dataAccess.ado.dialect`: `pgsql` (ADO path)
- `dataService.dataAccess.entities`: `immutable`
- `dataService.domain`: `default`

### Execution Slot Resolution

Orchestrator maps blueprint to slot activation:

- Presentation slot: `controllers` or `minimal-api`
- Data layer slot: `ef-core` or `ado-net`
- Tests slot: enabled unless `skipTests`

Selected slots determine which generators are included in execution plan via registry filtering.

---

## Metadata Schema Patterns

### BoundedContext Definition (DTO Layout Variant)

```json
{
  "boundedContexts": [
    {
      "name": "Commerce",
      "namespace": "Inventorization.Commerce",
      "apiPort": 5042,
      "dtoLayout": "class",
      "dataModel": {
        "entities": []
      }
    }
  ]
}
```

`dtoLayout` is optional. Supported values:

- `"class"` (default)
- `"record"`

### Entity Definition

```json
{
  "name": "Product",
  "tableName": "Products",
  "description": "Product in catalog",
  "auditable": true,
  "properties": [
    {
      "name": "Name",
      "type": "string",
      "required": true,
      "maxLength": 200,
      "description": "Product name",
      "validation": {
        "regex": "^[A-Za-z0-9 ]+$"
      }
    }
  ],
  "indexes": [
    { "columns": ["Name"], "isUnique": true }
  ]
}
```

### Relationship Definition

```json
{
  "type": "ManyToMany",
  "leftEntity": "Product",
  "rightEntity": "Category",
  "junctionEntity": "ProductCategory",
  "onDelete": "Restrict",
  "description": "Products can belong to multiple categories"
}
```

### Validation Rules

JSON Schema validates structure; business rules validated in `DataModelParser`:

```typescript
private validateBusinessRules(domain: DomainModel): void {
  // Check entity name uniqueness within each bounded context
  // Validate FK references point to existing entities
  // Ensure ManyToMany has junction entity
  // Validate enum value uniqueness (domain-level + context-level)
  // Context-level enums override domain-level on name collision
  // etc.
}
```

---

## Extension Patterns

### Adding a New Generator

1. **Create generator class** extending `BaseGenerator`
2. **Create Handlebars template(s)** under the appropriate concern/variant subdirectory in `templates/`
3. **Use fallback template arrays** (`new-path`, then optional `legacy-flat`) during migration windows
4. **Register in Orchestrator registry** with phase/dependencies/optionalSlot
5. **Update types** if new context structure needed

```typescript
// src/generators/ValidatorGenerator.ts
export class ValidatorGenerator extends BaseGenerator {
  async generate(model: BoundedContextGenerationContext): Promise<void> {
    const validatorsDir = path.join(
      `Inventorization.${model.boundedContext.name}.BL/Validators`
    );
    
    for (const entity of model.entities) {
      await this.generateCreateValidator(entity, validatorsDir);
      await this.generateUpdateValidator(entity, validatorsDir);
    }
  }
  
  private async generateCreateValidator(entity: Entity, dir: string): Promise<void> {
    const context = {
      namespace: '...',
      entityName: entity.name,
      validationRules: this.extractValidationRules(entity)
    };
    
    await this.writeRenderedTemplate(
      ['domain/validator/create.generated.cs.hbs', 'create-validator.generated.cs.hbs'],
      context,
      path.join(dir, `Create${entity.name}Validator.generated.cs`),
      true
    );
  }
}
```

### Adding Template Helpers

Register in `BaseGenerator.registerHelpers()`:

```typescript
Handlebars.registerHelper('pluralize', (word: string) => {
  return NamingConventions.pluralize(word);
});
```

Use in templates:

```handlebars
public DbSet<{{entityName}}> {{pluralize entityName}} => Set<{{entityName}}>();
```

---

## File Organization

### Generated Files Structure

```
Inventorization.{Context}.Meta/                   ← METADATA PROJECT
├── DataModelMetadata.cs                          ← Generated entity metadata
└── DataModelRelationships.cs                     ← Generated relationship metadata

Inventorization.{Context}.BL/
├── Entities/
│   ├── Product.cs                                ← GENERATED (can overwrite)
│   └── Category.cs                               ← GENERATED (can overwrite)
├── EntityConfigurations/
│   ├── ProductConfiguration.cs
│   └── CategoryConfiguration.cs
├── Creators/
│   └── ProductCreator.cs
├── Services/                                     ← CUSTOM DOMAIN LOGIC
│   ├── IProductDomainService.cs                  ← Developer-owned
│   └── ProductDomainService.cs                   ← Developer-owned
└── ... (other folders)

Inventorization.{Context}.DTO/
└── DTO/
    ├── Product/
    │   ├── CreateProductDTO.cs
    │   ├── UpdateProductDTO.cs
    │   ├── DeleteProductDTO.cs
    │   ├── ProductDetailsDTO.cs
    │   └── ProductSearchDTO.cs
    └── Category/
        └── ... (same 5 DTOs)
```

### File Naming Rules

- **Generated files**: Standard `.cs` extension (can be regenerated)
- **Custom domain services**: `{Entity}DomainService.cs` (developer-owned)
- **DTOs**: Named per pattern (`Create{Entity}DTO`, etc.)
- **Interfaces**: Start with `I` (`IProductDataService`)
- **Controllers**: Plural entity name (`ProductsController`)
- **Custom controllers**: Can extend generated base or implement custom endpoints

---

## Testing Strategy

### Generator Unit Tests

Test each generator independently:

```typescript
describe('EntityGenerator', () => {
  it('should generate immutable entity with validation', async () => {
    const model: BoundedContextGenerationContext = { /* test data */ };
    
    const generator = new EntityGenerator();
    await generator.generate(model);
    
    const content = await fs.readFile('/tmp/test/.../Product.generated.cs', 'utf-8');
    expect(content).toContain('public partial class Product');
    expect(content).toContain('private set');
  });
});
```

### Integration Tests

Test full generation pipeline:

```bash
npm start generate examples/test-model.json -- --output-dir /tmp/test
dotnet build /tmp/test/Inventorization.Test.Domain
# Should compile without errors
```

---

## Performance Considerations

### Template Caching

Templates are cached by **resolved template name** after first load:

```typescript
protected async loadTemplate(name: string | readonly string[]): Promise<HandlebarsTemplateDelegate> {
  const resolved = await this.resolveTemplateName(name);
  if (this.templates.has(resolved)) {
    return this.templates.get(resolved)!;  // Cache hit
  }
  // Load resolved template and cache by resolved key
}
```

### Parallel Generation

Currently sequential; could parallelize independent generators:

```typescript
// Future optimization
await Promise.all([
  entityGenerator.generate(model),
  dtoGenerator.generate(model),
  // Only if no dependencies between them
]);
```

### File I/O Batching

FileManager uses `fs-extra` for efficient I/O:

```typescript
await fs.ensureDir(dir);              // Creates parent dirs if needed
await fs.writeFile(path, content);    // Atomic write
```

---

## Error Handling

### Validation Errors

```typescript
// Schema validation
if (!valid) {
  throw new Error(`Data model validation failed:\n${errors.join('\n')}`);
}

// Business rule validation
if (!entityExists) {
  throw new Error(`Relationship references unknown entity: ${name}`);
}
```

### Generation Errors

```typescript
try {
  await generator.generate(model);
} catch (error) {
  console.error(chalk.red('Generation failed:'), error.message);
  process.exit(1);
}
```

---

## Smart Enum Generation

The generator creates **Smart Enum classes** instead of traditional C# enums to provide type-safe enumerations with string JSON serialization and integer database storage.

### Data Model Specification

Enums live in the `DomainModel` JSON. They can be declared at two levels:

- **Domain-level** (`enums` on the root): shared across all bounded contexts in the file.
- **Context-level** (`enums` inside a `boundedContexts` entry): scoped to that context only.

If the same enum name appears at both levels the context-level definition wins (override).

```json
{
  "enums": [
    {
      "name": "ProductStatus",
      "description": "Product availability status",
      "values": [
        { "name": "Draft", "value": 0, "description": "Product is being prepared" },
        { "name": "Active", "value": 1, "description": "Product is available for sale" },
        { "name": "OutOfStock", "value": 2, "description": "Temporarily unavailable" },
        { "name": "Discontinued", "value": 3, "description": "No longer available" }
      ]
    }
  ],
  "boundedContexts": [
    {
      "name": "Commerce",
      "namespace": "Inventorization.Commerce",
      "dataModel": {
        "entities": []
      }
    }
  ]
}
```

### Generated Smart Enum Class

**Output**: `Inventorization.{Context}.Common/Enums/ProductStatus.cs`

```csharp
[JsonConverter(typeof(EnumerationJsonConverter<ProductStatus>))]
public sealed class ProductStatus : Enumeration
{
    public static readonly ProductStatus Draft = new(nameof(Draft), 0);
    public static readonly ProductStatus Active = new(nameof(Active), 1);
    public static readonly ProductStatus OutOfStock = new(nameof(OutOfStock), 2);
    public static readonly ProductStatus Discontinued = new(nameof(Discontinued), 3);

    private ProductStatus(string name, int value) : base(name, value) { }

    public static ProductStatus FromName(string name) => FromNameOrThrow<ProductStatus>(name);
    public static ProductStatus FromValue(int value) => FromValueOrThrow<ProductStatus>(value);
    public static IEnumerable<ProductStatus> GetAll() => Enumeration.GetAll<ProductStatus>();
}
```

### Using Smart Enums in Entities

Reference enum type in property metadata:

```json
{
  "properties": [
    {
      "name": "Status",
      "type": "int",
      "enumType": "ProductStatus",
      "required": true,
      "description": "Product status",
      "defaultValue": "ProductStatus.Draft"
    }
  ]
}
```

**Generated Entity**:

```csharp
public class Product : BaseEntity
{
    public Product(
        string name,
        ProductStatus status  // ← Smart Enum type
    )
    {
        Status = status;
    }

    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
}
```

**Generated Entity Configuration**:

```csharp
public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
    {
        builder.Property(e => e.Status)
            .IsRequired()
            .HasDefaultValue(ProductStatus.Draft)
            .HasConversion(new EnumerationConverter<ProductStatus>());  // ← Stores as int
    }
}
```

**Generated Creator**:

```csharp
public class ProductCreator : IEntityCreator<Product, CreateProductDTO>
{
    public Task<Product> CreateAsync(CreateProductDTO dto, CancellationToken ct)
    {
        return Task.FromResult(new Product(
            dto.Name,
            dto.Status  // ← Direct pass-through (no int casting)
        ));
    }
}
```

### Template Implementation

**enum.cs.hbs**:
```handlebars
[JsonConverter(typeof(EnumerationJsonConverter<{{name}}>))]
public sealed class {{name}} : Enumeration
{
    {{#each values}}
    /// <summary>
    /// {{description}}
    /// </summary>
    public static readonly {{../name}} {{name}} = new(nameof({{name}}), {{value}});

    {{/each}}
    private {{name}}(string name, int value) : base(name, value) { }

    public static {{name}} FromName(string name) => FromNameOrThrow<{{name}}>(name);
    public static {{name}} FromValue(int value) => FromValueOrThrow<{{name}}>(value);
    public static IEnumerable<{{name}}> GetAll() => Enumeration.GetAll<{{name}}>();
}
```

### Generator Logic

**ConfigurationGenerator.ts** - Detects enum properties and adds converter:

```typescript
private getPropertyConfigurations(properties: Property[]): string[] {
  for (const prop of properties) {
    if (prop.enumType) {
      lines.push(`    .HasConversion(new EnumerationConverter<${prop.enumType}>())`);
    }
  }
}
```

**EntityGenerator.ts** - Uses enum type for constructor parameters:

```typescript
private getConstructorParams(properties: Property[]): Array<{type: string}> {
  return properties.map((p) => ({
    type: TypeMapper.toCSharpType(p.enumType || p.type, !p.required),  // ← enumType priority
  }));
}
```

**AbstractionGenerator.ts** - No int casting in creators:

```typescript
private getConstructorArgs(entity: Entity): Array<{argValue: string}> {
  return constructorProps.map((p) => ({
    argValue: `dto.${p.name}`,  // ← Direct pass-through (Smart Enums are reference types)
  }));
}
```

### API Behavior

**Request** (accepts both string and int):
```json
POST /api/products
{
  "name": "Widget",
  "status": "Active"  // ← or "status": 1
}
```

**Response** (always string):
```json
{
  "id": "guid",
  "name": "Widget",
  "status": "Active"  // ← Never numeric
}
```

**Database** (stores as integer):
```sql
CREATE TABLE Products (
  Status INT NOT NULL DEFAULT 0  -- 0 = Draft, 1 = Active, etc.
);
```

### Benefits

1. **API Readability** - `"Active"` instead of `1` in JSON
2. **Database Efficiency** - Integer storage and indexing
3. **Type Safety** - Compile-time checking
4. **Backward Compatible** - Accepts both string and int in requests
5. **Extensible** - Can add methods/properties to enum classes
6. **Descriptive** - XML doc comments on each value

### Limitations

**Cannot use in switch statements** - Use if-else chains:

```csharp
// ❌ Won't compile (Smart Enums are not compile-time constants)
switch (product.Status)
{
    case ProductStatus.Draft:
        break;
}

// ✅ Correct approach
if (product.Status == ProductStatus.Draft)
{
    // ...
}
```

---

## Future Enhancements

### Planned Features

1. **Incremental Generation** - Detect changes and regenerate only affected files
2. **Migration Generation** - Auto-generate EF Core migrations from schema changes
3. **GraphQL Schema** - Generate GraphQL schema from entities
4. **API Documentation** - Generate Swagger/OpenAPI specs from metadata
5. **Test Data Factories** - Generate test data builders
6. **Database Seeding** - Generate seed data scripts
7. **Localization** - Multi-language support in generated code

### Extensibility Points

- Custom template directories
- Plugin system for generators
- Custom validators for business rules
- Code formatters (Prettier for C#)
- Pre/post generation hooks

---

## Best Practices Summary

1. ✅ **Metadata is source of truth** - All generation driven by JSON data model
2. ✅ **Strong typing** - No `any` types; use `unknown` and type guards
3. ✅ **Separation of concerns** - Generated code = data model, custom logic = services
4. ✅ **Naming conventions** - Use `NamingConventions` class consistently
5. ✅ **Template simplicity** - Keep logic in TypeScript, templates dumb
6. ✅ **Dependency order** - Generate in correct sequence
7. ✅ **Regeneration allowed** - All generated files can be safely overwritten
8. ✅ **Idempotent** - Running generator multiple times produces same result
9. ✅ **Error messages** - Clear, actionable error messages with context
10. ✅ **Documentation** - XML comments on all generated public members
11. ✅ **Custom logic isolation** - Domain services handle business logic, not entities

---

## Related Documents

- [Architecture.md](Architecture.md) - Target architecture patterns
- [generation/code/README.md](generation/code/README.md) - Generator usage
- [generation/code/schemas/data-model.schema.json](generation/code/schemas/data-model.schema.json) - JSON Schema
- [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md) - Development history
