# BoundedContext Code Generator

Automated code generator for creating complete BoundedContext microservices in the Inventorization system.

## Features

- 🚀 **Full Project Generation** - Creates all layers: DTO, Domain, API, Tests
- 📋 **JSON Schema Input** - Type-safe data model definitions
- 🔄 **Partial Classes** - Separates generated code from custom logic
- ✅ **Test Scaffolding** - Generates unit test stubs
- 🐳 **Docker Integration** - Updates docker-compose.yml automatically
- 🎯 **Architecture Compliance** - Follows patterns from Architecture.md

## Installation

```bash
cd generation/code
npm install
npm run build
```

## Usage

### Generate a BoundedContext

```bash
npm start generate examples/simple-bounded-context.json -- --output-dir ../../backend
```

### Validate Data Model

```bash
npm start validate examples/simple-bounded-context.json
```

### CLI Options

- `--output-dir <path>` - Target directory for generated code (default: `../../backend`)
- `--namespace <name>` - Override namespace prefix (default: from JSON)
- `--skip-tests` - Don't generate test projects
- `--dry-run` - Preview what would be generated without writing files
- `--force` - Overwrite existing .generated.cs files

## Data Model Format

See [schemas/data-model.schema.json](schemas/data-model.schema.json) for complete specification.

Example:

```json
{
  "boundedContext": {
    "name": "Products",
    "namespace": "Inventorization.Products",
    "description": "Product catalog management"
  },
  "entities": [
    {
      "name": "Product",
      "tableName": "Products",
      "description": "Product in catalog",
      "properties": [
        {
          "name": "Name",
          "type": "string",
          "required": true,
          "maxLength": 200,
          "description": "Product name"
        },
        {
          "name": "Price",
          "type": "decimal",
          "required": true,
          "precision": 18,
          "scale": 2
        }
      ]
    }
  ],
  "relationships": [
    {
      "type": "ManyToMany",
      "leftEntity": "Product",
      "rightEntity": "Category",
      "junctionEntity": "ProductCategory"
    }
  ]
}
```

## Generated Project Structure

```
Inventorization.{Context}.DTO/
├── DTO/{Entity}/
│   ├── Create{Entity}DTO.cs
│   ├── Update{Entity}DTO.cs
│   ├── Delete{Entity}DTO.cs
│   ├── {Entity}DetailsDTO.cs
│   └── {Entity}SearchDTO.cs

Inventorization.{Context}.Domain/
├── Entities/
│   ├── {Entity}.generated.cs
│   └── {Entity}.cs (custom logic stub)
├── EntityConfigurations/
│   └── {Entity}Configuration.generated.cs
├── Creators/
│   └── {Entity}Creator.generated.cs
├── Modifiers/
│   └── {Entity}Modifier.generated.cs
├── Mappers/
│   └── {Entity}Mapper.generated.cs
├── SearchProviders/
│   └── {Entity}SearchProvider.generated.cs
├── Validators/
│   ├── Create{Entity}Validator.generated.cs
│   └── Update{Entity}Validator.generated.cs
├── DataServices/
│   └── {Entity}DataService.generated.cs
├── DbContexts/
│   └── {Context}DbContext.generated.cs
├── DataAccess/
│   └── {Context}UnitOfWork.generated.cs
├── DataModelMetadata.generated.cs
└── DataModelRelationships.generated.cs

Inventorization.{Context}.API/
├── Controllers/
│   ├── {Entity}sController.generated.cs
│   └── {Entity}sController.cs (custom endpoints stub)
└── Program.generated.cs (DI registrations)

Inventorization.{Context}.API.Tests/
└── (test scaffolds)
```

## Custom Logic

Add custom logic in non-generated files (without `.generated.cs` suffix):

- **{Entity}.cs** - Custom mutation methods, business logic
- **{Entity}Validator.cs** - Complex validation rules
- **{Entity}sController.cs** - Additional API endpoints

These files are never overwritten during regeneration.

## Architecture Compliance

Generated code follows all rules from [Architecture.md](../../Architecture.md):

- ✅ Base abstractions from Inventorization.Base
- ✅ Dependency injection with interfaces
- ✅ Immutable entities with private setters
- ✅ Generic data services
- ✅ IMapper abstraction for object mapping
- ✅ DTO inheritance from base DTOs
- ✅ Entity configurations with fluent API
- ✅ Unit test scaffolding

## Development

```bash
# Watch mode
npm run dev

# Format code
npm run format

# Lint
npm run lint

# Clean build artifacts
npm run clean
```

## License

MIT
