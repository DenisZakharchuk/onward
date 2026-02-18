using Inventorization.Base.Abstractions;
using Inventorization.Base.Models;
using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL;

/// <summary>
/// Centralized repository of all entity metadata for the Goods bounded context.
/// Single source of truth for entity structure, validation rules, and EF configuration.
/// Use this metadata for:
/// - Generating validators
/// - Configuring EF Core models
/// - Generating UI forms
/// - API documentation
/// - Code generation
/// </summary>
internal static class DataModelMetadata
{
    #region Good Entity Metadata

    /// <summary>
    /// Complete metadata for the Good entity
    /// </summary>
    public static readonly IDataModelMetadata<Good> Good = new DataModelMetadataBuilder<Good>()
        .WithTable("Goods", schema: null)
        .WithDisplayName("Good")
        .WithDescription("Represents a product/item in the inventory system")
        .WithAuditing()
        .AddProperties(
            // Primary Key
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.Id),
                propertyType: typeof(Guid),
                displayName: "ID",
                isPrimaryKey: true,
                isRequired: true,
                description: "Unique identifier for the good"),

            // Name
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.Name),
                propertyType: typeof(string),
                displayName: "Name",
                isRequired: true,
                maxLength: 200,
                description: "Name of the good",
                validationMessage: "Name is required and must not exceed 200 characters"),

            // Description
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.Description),
                propertyType: typeof(string),
                displayName: "Description",
                isRequired: false,
                maxLength: 1000,
                description: "Detailed description of the good"),

            // SKU
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.Sku),
                propertyType: typeof(string),
                displayName: "SKU",
                isRequired: true,
                maxLength: 50,
                isUnique: true,
                isIndexed: true,
                description: "Stock Keeping Unit - unique identifier",
                validationMessage: "SKU is required, must be unique, and not exceed 50 characters"),

            // Unit Price
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.UnitPrice),
                propertyType: typeof(decimal),
                displayName: "Unit Price",
                columnType: "decimal(18,2)",
                isRequired: true,
                precision: 18,
                scale: 2,
                minValue: 0m,
                description: "Price per unit",
                validationMessage: "Unit price is required and must be non-negative"),

            // Quantity In Stock
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.QuantityInStock),
                propertyType: typeof(int),
                displayName: "Quantity in Stock",
                isRequired: true,
                minValue: 0,
                defaultValue: 0,
                description: "Current quantity available in stock",
                validationMessage: "Quantity must be non-negative"),

            // Unit of Measure
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.UnitOfMeasure),
                propertyType: typeof(string),
                displayName: "Unit of Measure",
                isRequired: false,
                maxLength: 20,
                description: "Unit of measurement (e.g., 'pcs', 'kg', 'liter')"),

            // Category ID (Foreign Key)
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.CategoryId),
                propertyType: typeof(Guid?),
                displayName: "Category",
                isRequired: false,
                isForeignKey: true,
                isIndexed: true,
                description: "Optional category this good belongs to"),

            // Is Active
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.IsActive),
                propertyType: typeof(bool),
                displayName: "Active",
                isRequired: true,
                defaultValue: true,
                description: "Whether this good is active in the system"),

            // Created At (Audit)
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.CreatedAt),
                propertyType: typeof(DateTime),
                displayName: "Created At",
                isRequired: true,
                defaultValueSql: "GETUTCDATE()",
                description: "Timestamp when the good was created"),

            // Updated At (Audit)
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Good.UpdatedAt),
                propertyType: typeof(DateTime?),
                displayName: "Updated At",
                isRequired: false,
                description: "Timestamp when the good was last updated")
        )
        .WithPrimaryKey(nameof(Entities.Good.Id))
        .AddIndex(nameof(Entities.Good.Sku))
        .AddIndex(nameof(Entities.Good.CategoryId))
        .AddIndex(nameof(Entities.Good.IsActive))
        .AddUniqueConstraint(nameof(Entities.Good.Sku))
        .AddRelationships(
            DataModelRelationships.GoodSuppliers,
            DataModelRelationships.GoodStockItems,
            DataModelRelationships.GoodPurchaseOrderItems)
        .Build();

    #endregion

    #region Category Entity Metadata

    /// <summary>
    /// Complete metadata for the Category entity
    /// </summary>
    public static readonly IDataModelMetadata<Category> Category = new DataModelMetadataBuilder<Category>()
        .WithTable("Categories", schema: null)
        .WithDisplayName("Category")
        .WithDescription("Product category for organizational hierarchy")
        .WithAuditing()
        .AddProperties(
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.Id),
                propertyType: typeof(Guid),
                isPrimaryKey: true,
                isRequired: true,
                description: "Unique identifier for the category"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.Name),
                propertyType: typeof(string),
                displayName: "Name",
                isRequired: true,
                maxLength: 100,
                description: "Category name"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.Description),
                propertyType: typeof(string),
                displayName: "Description",
                isRequired: false,
                maxLength: 500,
                description: "Category description"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.ParentCategoryId),
                propertyType: typeof(Guid?),
                displayName: "Parent Category",
                isRequired: false,
                isForeignKey: true,
                isIndexed: true,
                description: "Parent category for hierarchical structure"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.IsActive),
                propertyType: typeof(bool),
                displayName: "Active",
                isRequired: true,
                defaultValue: true,
                description: "Whether this category is active"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.CreatedAt),
                propertyType: typeof(DateTime),
                isRequired: true,
                defaultValueSql: "GETUTCDATE()",
                description: "Timestamp when created"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Category.UpdatedAt),
                propertyType: typeof(DateTime?),
                isRequired: false,
                description: "Timestamp when last updated")
        )
        .WithPrimaryKey(nameof(Entities.Category.Id))
        .AddIndex(nameof(Entities.Category.ParentCategoryId))
        .AddIndex(nameof(Entities.Category.IsActive))
        .AddRelationships(
            DataModelRelationships.CategoryGoods,
            DataModelRelationships.CategorySubCategories)
        .Build();

    #endregion

    #region Supplier Entity Metadata

    /// <summary>
    /// Complete metadata for the Supplier entity
    /// </summary>
    public static readonly IDataModelMetadata<Supplier> Supplier = new DataModelMetadataBuilder<Supplier>()
        .WithTable("Suppliers", schema: null)
        .WithDisplayName("Supplier")
        .WithDescription("Supplier/vendor information")
        .WithAuditing()
        .AddProperties(
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.Id),
                propertyType: typeof(Guid),
                isPrimaryKey: true,
                isRequired: true,
                description: "Unique identifier"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.Name),
                propertyType: typeof(string),
                displayName: "Name",
                isRequired: true,
                maxLength: 200,
                description: "Supplier name"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.ContactEmail),
                propertyType: typeof(string),
                displayName: "Contact Email",
                isRequired: true,
                maxLength: 100,
                regexPattern: @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                description: "Contact email address",
                validationMessage: "Must be a valid email address"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.ContactPhone),
                propertyType: typeof(string),
                displayName: "Contact Phone",
                isRequired: false,
                maxLength: 20,
                description: "Contact phone number"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.Address),
                propertyType: typeof(string),
                displayName: "Address",
                isRequired: false,
                maxLength: 500,
                description: "Physical address"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.IsActive),
                propertyType: typeof(bool),
                displayName: "Active",
                isRequired: true,
                defaultValue: true,
                description: "Whether this supplier is active"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.CreatedAt),
                propertyType: typeof(DateTime),
                isRequired: true,
                defaultValueSql: "GETUTCDATE()",
                description: "Timestamp when created"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Supplier.UpdatedAt),
                propertyType: typeof(DateTime?),
                isRequired: false,
                description: "Timestamp when last updated")
        )
        .WithPrimaryKey(nameof(Entities.Supplier.Id))
        .AddIndex(nameof(Entities.Supplier.IsActive))
        .AddIndex(nameof(Entities.Supplier.ContactEmail))
        .AddRelationships(
            DataModelRelationships.GoodSuppliers,
            DataModelRelationships.SupplierPurchaseOrders)
        .Build();

    #endregion

    #region Warehouse Entity Metadata

    /// <summary>
    /// Complete metadata for the Warehouse entity
    /// </summary>
    public static readonly IDataModelMetadata<Warehouse> Warehouse = new DataModelMetadataBuilder<Warehouse>()
        .WithTable("Warehouses", schema: null)
        .WithDisplayName("Warehouse")
        .WithDescription("Physical warehouse/storage facility")
        .WithAuditing()
        .AddProperties(
            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.Id),
                propertyType: typeof(Guid),
                isPrimaryKey: true,
                isRequired: true,
                description: "Unique identifier"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.Name),
                propertyType: typeof(string),
                displayName: "Name",
                isRequired: true,
                maxLength: 200,
                description: "Warehouse name"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.Code),
                propertyType: typeof(string),
                displayName: "Code",
                isRequired: true,
                maxLength: 20,
                isUnique: true,
                isIndexed: true,
                description: "Unique warehouse code"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.Address),
                propertyType: typeof(string),
                displayName: "Address",
                isRequired: false,
                maxLength: 500,
                description: "Physical address"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.IsActive),
                propertyType: typeof(bool),
                displayName: "Active",
                isRequired: true,
                defaultValue: true,
                description: "Whether this warehouse is active"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.CreatedAt),
                propertyType: typeof(DateTime),
                isRequired: true,
                defaultValueSql: "GETUTCDATE()",
                description: "Timestamp when created"),

            new DataPropertyMetadata(
                propertyName: nameof(Entities.Warehouse.UpdatedAt),
                propertyType: typeof(DateTime?),
                isRequired: false,
                description: "Timestamp when last updated")
        )
        .WithPrimaryKey(nameof(Entities.Warehouse.Id))
        .AddIndex(nameof(Entities.Warehouse.Code))
        .AddIndex(nameof(Entities.Warehouse.IsActive))
        .AddUniqueConstraint(nameof(Entities.Warehouse.Code))
        .AddRelationship(DataModelRelationships.WarehouseStockLocations)
        .Build();

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get all entity metadata in this bounded context
    /// </summary>
    public static IReadOnlyList<IDataModelMetadata> GetAllEntityMetadata() => new List<IDataModelMetadata>
    {
        Good,
        Category,
        Supplier,
        Warehouse
        // Add more entities as they are defined
    };

    /// <summary>
    /// Get metadata for a specific entity type
    /// </summary>
    public static IDataModelMetadata? GetEntityMetadata<TEntity>() where TEntity : class
    {
        return GetAllEntityMetadata().FirstOrDefault(m => m.EntityType == typeof(TEntity));
    }

    /// <summary>
    /// Get metadata for a specific entity by name
    /// </summary>
    public static IDataModelMetadata? GetEntityMetadata(string entityName)
    {
        return GetAllEntityMetadata().FirstOrDefault(m => 
            m.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}
