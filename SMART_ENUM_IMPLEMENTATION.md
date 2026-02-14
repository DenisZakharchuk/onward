# Smart Enum Pattern Implementation

## Overview
Replaced traditional C# enums with **Smart Enums** (Enumeration Classes) for:
- `ProductStatus`
- `OrderStatus`
- `AddressType`

## Architecture

### Base Class
**Location**: `Inventorization.Base/Models/Enumeration.cs`

```csharp
public abstract class Enumeration : IComparable<Enumeration>, IEquatable<Enumeration>
{
    public string Name { get; }  // e.g., "Active"
    public int Value { get; }    // e.g., 1
}
```

### Example: ProductStatus
**Location**: `Inventorization.Commerce.Common/Enums/ProductStatus.cs`

```csharp
[JsonConverter(typeof(EnumerationJsonConverter<ProductStatus>))]
public sealed class ProductStatus : Enumeration
{
    public static readonly ProductStatus Draft = new(nameof(Draft), 0);
    public static readonly ProductStatus Active = new(nameof(Active), 1);
    public static readonly ProductStatus OutOfStock = new(nameof(OutOfStock), 2);
    public static readonly ProductStatus Discontinued = new(nameof(Discontinued), 3);

    private ProductStatus(string name, int value) : base(name, value) { }
}
```

## Serialization Behavior

### Database Storage
**Type**: `int`
**Example**: `1` (stored as integer for efficiency)

**EF Core Configuration**:
```csharp
builder.Property(e => e.Status)
    .HasConversion(new EnumerationConverter<ProductStatus>());
```

### API JSON Response
**Type**: `string`
**Example**: `"Active"` (human-readable)

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Gaming Laptop",
  "status": "Active",
  "price": 1299.99
}
```

### API JSON Request
**Accepts both**:
- String: `{"status": "Active"}`
- Integer: `{"status": 1}`

**JSON Converter** handles automatic deserialization.

## Usage in Code

### Entity Properties
```csharp
public class Product : BaseEntity
{
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    
    public void Activate() 
    { 
        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Comparisons
```csharp
// Equality
if (dto.Status == ProductStatus.Active) { ... }

// Pattern matching (if-else, not switch/case)
if (dto.Status == ProductStatus.Active)
    entity.Activate();
else if (dto.Status == ProductStatus.OutOfStock)
    entity.MarkOutOfStock();
```

⚠️ **Note**: Cannot use in `switch` statements (not compile-time constants). Use `if-else` chains instead.

### Getting All Values
```csharp
var allStatuses = ProductStatus.GetAll();
// Returns: [Draft, Active, OutOfStock, Discontinued]
```

### Parsing
```csharp
// From string (case-insensitive)
var status = ProductStatus.FromName("active"); // Returns ProductStatus.Active

// From int
var status = ProductStatus.FromValue(1); // Returns ProductStatus.Active

// Throws ArgumentException if invalid
```

## Benefits vs Traditional Enums

| Feature | Traditional Enum | Smart Enum |
|---------|-----------------|------------|
| API JSON | `1` (number) ❌ | `"Active"` (string) ✅ |
| DB Storage | `1` (int) ✅ | `1` (int) ✅ |
| Type Safety | ✅ | ✅ |
| Can add methods | ❌ | ✅ |
| Switch statements | ✅ | ❌ |
| DDD friendly | Partial | ✅ |

## Components Added

1. **Base Class**: `Inventorization.Base/Models/Enumeration.cs`
2. **EF Converter**: `Inventorization.Base/DataAccess/EnumerationConverter.cs`
3. **JSON Converter**: `Inventorization.Base/Models/EnumerationJsonConverter.cs`
4. **Smart Enums**:
   - `ProductStatus` (4 values)
   - `OrderStatus` (5 values)
   - `AddressType` (3 values)

## Migration Impact

✅ **Backward Compatible**: Same int values stored in database
✅ **No data migration needed**: Existing data works as-is
✅ **API Breaking Change**: Responses now return strings instead of ints

### Before
```json
{"status": 1}
```

### After
```json
{"status": "Active"}
```

## Generator Updates Needed

When updating the code generator, ensure:
1. Import `Inventorization.Base.Models` for Enumeration
2. Add `[JsonConverter]` attribute to Smart Enum classes
3. Add `.HasConversion(new EnumerationConverter<T>())` to EF configurations
4. Use `if-else` instead of `switch` for enum comparisons in generated code
5. Keep int values for database efficiency

## Testing

To verify Smart Enum behavior:
```bash
# Start API
cd backend/Inventorization.Commerce.API
dotnet run

# Test endpoint
curl http://localhost:5022/api/products

# Response should show:
# "status": "Active"  ← String, not number!
```
