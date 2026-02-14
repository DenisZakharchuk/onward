# TransformationResult - Type-Safe Dynamic Projections

## Overview

`TransformationResult` provides type-safe dynamic projections for field transformations. It extends `Dictionary<string, object?>` with schema metadata to ensure type safety while maintaining flexibility.

## Key Concept

The output schema is **constrained but dynamic**:
- **Constrained by**: Request aliases + Entity model types + Transformation type inference
- **Dynamic**: Each request has a unique schema based on its transformation definitions
- **Not arbitrary**: Schema is deterministic given the projection request

This is the "superposition" of user-defined field names and bounded context data types.

## Architecture

### TransformationResult Class

Located: `Inventorization.Base/Models/TransformationResult.cs`

```csharp
public class TransformationResult : Dictionary<string, object?>
{
    // Schema metadata - field name → expected type
    public IReadOnlyDictionary<string, Type> Schema { get; }
    
    // Type-safe accessors
    public T? GetTypedValue<T>(string fieldName);  // Throws on type mismatch
    public bool TryGetTypedValue<T>(string fieldName, out T? value);  // Safe access
    
    // Schema introspection
    public bool HasField(string fieldName);
    public Type? GetFieldType(string fieldName);
    
    // OpenAPI/Swagger support
    public Dictionary<string, string> GetOpenApiSchema();  // For documentation
}
```

### Schema Inference

The schema is inferred from `ProjectionField.GetOutputType()`:

| Transformation Type | Output Type |
|---------------------|-------------|
| `FieldReference` | Entity field type (e.g., `string`, `decimal`, `Guid`) |
| `ConstantValue` | Constant type (e.g., `string`, `int`, `bool`) |
| `StringTransform` | `string` |
| `ConcatTransform` | `string` |
| `ArithmeticTransform` | `decimal` |
| `ComparisonTransform` | `bool` |
| `ConditionalTransform` | `object` (union of then/else types) |
| `CoalesceTransform` | Primary type (first non-null) |
| `ObjectConstruction` | Anonymous object type |
| `TypeCast` | Target type |

### Expression Builder

Located: `Inventorization.Base/Services/ProjectionExpressionBuilder.cs`

```csharp
public class ProjectionExpressionBuilder
{
    public Expression<Func<TEntity, TransformationResult>> BuildTransformationExpression<TEntity>(
        IReadOnlyDictionary<string, ProjectionField> transformations)
        where TEntity : class
    {
        // Step 1: Infer schema from transformations
        var schema = new Dictionary<string, Type>();
        foreach (var (fieldName, projectionField) in transformations)
        {
            schema[fieldName] = projectionField.GetOutputType();
        }
        
        // Step 2: Create TransformationResult with schema
        var result = new TransformationResult(schema);
        
        // Step 3: Build transformation expressions
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        foreach (var (fieldName, projectionField) in transformations)
        {
            var valueExpression = BuildExpression<TEntity>(projectionField, parameter);
            result[fieldName] = valueExpression;  // Validates against schema
        }
        
        // Step 4: Return typed result
        return Expression.Lambda<Func<TEntity, TransformationResult>>(
            Expression.Constant(result), parameter);
    }
}
```

## Integration with Search Services

### Current State

Projection mappers (e.g., `GoodProjectionMapper`) handle **regular projections only**:
- Input: `ProjectionRequest` with `Fields` or `IsAllFields`
- Output: `Expression<Func<Good, GoodProjection>>`
- Result: `SearchResult<GoodProjection>`

### Transformation Projections

Transformations require **separate handling**:
1. Detect `ProjectionRequest.FieldTransformations != null`
2. Use `ProjectionExpressionBuilder.BuildTransformationExpression()` directly
3. Return `SearchResult<TransformationResult>`

### Implementation Pattern

#### Option 1: Dual Search Methods

```csharp
public class GoodSearchService
{
    private readonly IRepository<Good> _repository;
    private readonly IGoodProjectionMapper _projectionMapper;
    private readonly ProjectionExpressionBuilder _expressionBuilder;
    
    public async Task<ServiceResult<SearchResult<GoodProjection>>> ExecuteSearchAsync(
        SearchQuery query, CancellationToken ct = default)
    {
        // Regular projections
        var projection = _projectionMapper.GetProjectionExpression(query.Projection);
        var items = await _repository.GetQueryable()
            .Select(projection)
            .ToListAsync(ct);
        // ...
    }
    
    public async Task<ServiceResult<SearchResult<TransformationResult>>> ExecuteTransformationSearchAsync(
        SearchQuery query, CancellationToken ct = default)
    {
        // Transformation projections
        var projection = _expressionBuilder.BuildTransformationExpression<Good>(
            query.Projection.FieldTransformations!);
        var items = await _repository.GetQueryable()
            .Select(projection)
            .ToListAsync(ct);
        // ...
    }
}
```

#### Option 2: Unified Search with Type Detection

```csharp
public class GoodSearchService
{
    public async Task<ServiceResult<SearchResult<object>>> ExecuteSearchAsync(
        SearchQuery query, CancellationToken ct = default)
    {
        if (query.Projection?.FieldTransformations != null)
        {
            // Transformation mode
            var projection = _expressionBuilder.BuildTransformationExpression<Good>(
                query.Projection.FieldTransformations);
            var items = await _repository.GetQueryable()
                .Select(projection)
                .ToListAsync(ct);
            return ServiceResult<SearchResult<object>>.Success(
                new SearchResult<object> { Items = items.Cast<object>().ToList() });
        }
        else
        {
            // Regular projection mode
            var projection = _projectionMapper.GetProjectionExpression(query.Projection);
            var items = await _repository.GetQueryable()
                .Select(projection)
                .ToListAsync(ct);
            return ServiceResult<SearchResult<object>>.Success(
                new SearchResult<object> { Items = items.Cast<object>().ToList() });
        }
    }
}
```

#### Option 3: Separate Endpoints

```csharp
[HttpPost("search")]
public async Task<ActionResult<SearchResult<GoodProjection>>> Search(
    [FromBody] SearchQuery query)
{
    var result = await _searchService.ExecuteSearchAsync(query);
    return Ok(result.Data);
}

[HttpPost("search/transform")]
public async Task<ActionResult<SearchResult<TransformationResult>>> SearchWithTransformations(
    [FromBody] SearchQuery query)
{
    var result = await _transformationSearchService.ExecuteSearchAsync(query);
    return Ok(result.Data);
}
```

## Usage Example

### Request (HTTP)

```http
POST http://localhost:5022/api/goods/search
Content-Type: application/json

{
  "filter": null,
  "sorting": null,
  "pagination": { "pageNumber": 1, "pageSize": 10 },
  "projection": {
    "fieldTransformations": {
      "upperName": { "field": "name", "operation": "upper" },
      "totalValue": { "left": "unitPrice", "right": "quantityInStock", "operation": "multiply" },
      "isExpensive": { "left": "unitPrice", "right": 100, "operator": "gt" },
      "displayName": [
        { "condition": { "left": "name", "right": null, "operator": "neq" }, "then": "name", "else": "Unnamed Good" }
      ]
    }
  }
}
```

### Response

```json
{
  "items": [
    {
      "upperName": "LAPTOP",
      "totalValue": 12500.00,
      "isExpensive": true,
      "displayName": "Laptop"
    },
    {
      "upperName": "MOUSE",
      "totalValue": 250.00,
      "isExpensive": false,
      "displayName": "Mouse"
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 10
}
```

### Access in C#

```csharp
var result = await searchService.ExecuteTransformationSearchAsync(query);
foreach (var item in result.Data.Items)
{
    // Type-safe access with validation
    string upperName = item.GetTypedValue<string>("upperName");
    decimal totalValue = item.GetTypedValue<decimal>("totalValue");
    bool isExpensive = item.GetTypedValue<bool>("isExpensive");
    
    // Safe access without exceptions
    if (item.TryGetTypedValue<string>("displayName", out var displayName))
    {
        Console.WriteLine($"{displayName}: {totalValue:C}");
    }
    
    // Schema introspection
    Console.WriteLine($"Schema: {string.Join(", ", item.Schema.Select(kv => $"{kv.Key}: {kv.Value.Name}"))}");
    // Output: "Schema: upperName: String, totalValue: Decimal, isExpensive: Boolean, displayName: String"
}
```

## OpenAPI/Swagger Documentation

TransformationResult can generate dynamic schemas for Swagger:

```csharp
var schema = transformationResult.GetOpenApiSchema();
// Returns: { "upperName": "string", "totalValue": "number", "isExpensive": "boolean", "displayName": "string" }
```

This enables:
- Auto-generated API documentation
- Client code generation with proper types
- Runtime schema validation

## Performance

- **Dictionary-level performance**: No dynamic/ExpandoObject overhead
- **Minimal schema cost**: Schema metadata is Dictionary<string, Type>
- **EF Core translation**: Transformations translate to SQL where possible
- **Client evaluation**: Complex transformations may execute client-side

## Type Safety Guarantees

### Compile-Time
- ❌ Not possible - schemas are runtime-dynamic based on request

### Runtime
- ✅ Schema validation on `GetTypedValue<T>()`
- ✅ Type checking on indexer set operations
- ✅ Clear exception messages on type mismatches
- ✅ Safe access via `TryGetTypedValue<T>()`

### Schema Validation Example

```csharp
var result = new TransformationResult(new Dictionary<string, Type>
{
    ["upperName"] = typeof(string),
    ["totalValue"] = typeof(decimal)
});

result["upperName"] = "LAPTOP";  // ✅ OK
result["totalValue"] = 1250.00m;  // ✅ OK

result["upperName"] = 123;  // ❌ Throws: Type mismatch for field 'upperName': expected String, got Int32
result["unknownField"] = "foo";  // ❌ Throws: Field 'unknownField' not in schema
```

## Next Steps

1. **Implement search service integration** - Choose one of the three patterns above
2. **Add controller endpoints** - Support transformation requests
3. **Add OpenAPI schema generation** - Dynamic schema documentation
4. **Test EF Core translation** - Verify which transformations translate to SQL
5. **Performance benchmarking** - Compare to regular projections
6. **Add unit tests** - Test schema validation and type safety
7. **Add integration tests** - Test full transformation pipeline

## Design Decisions

### Why extend Dictionary instead of composition?

- Direct compatibility with LINQ and serialization
- No wrapper overhead
- Natural indexer syntax
- Dictionary is the conceptual model

### Why runtime validation instead of compile-time?

- Schemas are dynamic (based on request)
- No way to generate types at compile-time from JSON
- Runtime validation provides clear error messages
- Type inference from ProjectionField.GetOutputType() is reliable

### Why not dynamic/ExpandoObject?

- User rejected as anti-pattern
- Loses type information
- Poor discoverability
- No compile-time checking support (even runtime fails)
- Swagger/OpenAPI integration difficult

### Why not predefined result types?

- Each transformation request has unique schema
- Predefined types would require code generation per request
- Not universal - tied to specific transformation examples
- Dictionary extension provides flexibility + type safety

## Related Files

- `Inventorization.Base/Models/TransformationResult.cs` - Result type implementation
- `Inventorization.Base/Services/ProjectionExpressionBuilder.cs` - Expression builder
- `Inventorization.Base/ADTs/ProjectionField.cs` - ADT hierarchy with GetOutputType()
- `Inventorization.Base/ADTs/Converters/ProjectionFieldConverter.cs` - JSON deserialization
- `backend/Inventorization.Goods.API/GoodsSearchExamples.http` - Examples 21-30

## Conclusion

TransformationResult achieves the goal of type-safe dynamic projections:

✅ **Type safety**: Runtime validation with clear error messages  
✅ **Flexibility**: Each request has unique schema  
✅ **Discoverability**: Schema metadata + OpenAPI generation  
✅ **Performance**: Dictionary-level, no dynamic overhead  
✅ **Constrained**: Schema bounded by request + entity model  
✅ **Extensible**: Easy to add new transformation types  

The design respects the user's insight: "Output is dynamic but not arbitrary - it's a superposition of request aliases and bounded context data model."
