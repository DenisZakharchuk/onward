# Phase 4 - V2 Endpoints Quick Reference

## Overview
All three controllers now have new **V2 endpoints** that use the modern `IDataService` pattern with consistent error handling through `ServiceResult<T>`.

## ProductsController V2 Endpoints

### Get Product by ID
```http
GET /api/products/v2/{id}
```
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "name": "Product Name",
    "price": 99.99,
    "categoryId": "guid",
    "stockQuantity": 100,
    "createdAt": "2025-01-31T10:00:00Z"
  },
  "message": null,
  "errors": []
}
```

### Create Product
```http
POST /api/products/v2
Content-Type: application/json

{
  "name": "New Product",
  "price": 99.99,
  "categoryId": "guid",
  "stockQuantity": 100
}
```

### Update Product
```http
PUT /api/products/v2/{id}
Content-Type: application/json

{
  "id": "guid",
  "name": "Updated Product",
  "price": 149.99
}
```

### Delete Product
```http
DELETE /api/products/v2/{id}
```

---

## CategoriesController V2 Endpoints

### Get Category by ID
```http
GET /api/categories/v2/{id}
```

### Create Category
```http
POST /api/categories/v2
Content-Type: application/json

{
  "name": "New Category"
}
```

### Update Category
```http
PUT /api/categories/v2/{id}
Content-Type: application/json

{
  "id": "guid",
  "name": "Updated Category"
}
```

### Delete Category
```http
DELETE /api/categories/v2/{id}
```

---

## StockController V2 Endpoints

### Create Stock Movement
```http
POST /api/stock/v2
Content-Type: application/json

{
  "productId": "guid",
  "type": 0,  // 0 = In, 1 = Out
  "quantity": 50,
  "notes": "Stock adjustment"
}
```

### Delete Stock Movement
```http
DELETE /api/stock/v2/{id}
```

---

## Error Response Format

All V2 endpoints return consistent error responses:

### Bad Request (400)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Validation error message",
  "errors": ["Field validation error 1", "Field validation error 2"]
}
```

### Not Found (404)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Product not found",
  "errors": []
}
```

### Server Error (500)
```json
{
  "isSuccess": false,
  "data": null,
  "message": "Error processing request",
  "errors": ["Error details"]
}
```

---

## Key Differences from V1

| Aspect | V1 | V2 |
|--------|-----|-----|
| Response Format | Direct entity or array | Wrapped in `ServiceResult<T>` |
| Error Handling | HTTP status codes only | Structured error messages + HTTP status |
| ID Validation | N/A | ID mismatch validation in PUT |
| Consistency | Varies by endpoint | Unified across all endpoints |
| Backward Compat | Original endpoints remain | V1 endpoints still available |

---

## Migration Path

**Phase 1 (Current):** V2 endpoints available alongside V1
- Clients can opt-in to new pattern
- No breaking changes
- Tests ensure stability

**Phase 2:** Frontend migrated to V2 endpoints
- Monitor metrics for any issues
- Ensure V2 stability in production

**Phase 3:** V1 endpoints deprecated
- Add deprecation warnings to V1
- Document migration timeline

**Phase 4:** V1 endpoints removed
- Clean up legacy code
- Consolidate to single pattern

---

## Benefits of V2 Endpoints

✅ **Consistent Response Format** - All endpoints return `ServiceResult<T>`
✅ **Better Error Handling** - Structured error information
✅ **Type Safety** - Strong typing with generic DTOs
✅ **Audit Trail** - Automatic logging to MongoDB
✅ **Testability** - Service layer is fully injectable
✅ **Scalability** - Ready for pagination and filtering
✅ **No Breaking Changes** - Backward compatible with V1

---

## Development Notes

### New Delete DTOs
To support the new pattern, concrete Delete DTO classes were created:
- `DeleteProductDTO`
- `DeleteCategoryDTO`
- `DeleteStockMovementDTO`

These inherit from the base `DeleteDTO` class and can be extended with entity-specific properties if needed in the future.

### Service Registration
All services are registered in `Program.cs` with the DI container:
```csharp
builder.Services.AddScoped<IProductService, ProductDataService>();
builder.Services.AddScoped<ICategoryService, CategoryDataService>();
builder.Services.AddScoped<IStockMovementService, StockMovementDataService>();
```

### Audit Logging
All V2 endpoints automatically generate audit logs in MongoDB:
- Action: ProductCreated, ProductUpdated, ProductDeleted, etc.
- EntityType: Product, Category, StockMovement
- Changes: Before/after diff
- Timestamp: Operation time in UTC
- UserId: Current user (from HTTP context)

