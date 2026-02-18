using Inventorization.Base.Abstractions;
using Inventorization.Base.Models;
using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL;

/// <summary>
/// Centralized repository of all relationship metadata for the Goods bounded context.
/// Single source of truth for entity relationships - used by relationship managers,
/// configurations, and DI registration.
/// </summary>
public static class DataModelRelationships
{
    /// <summary>
    /// Good ↔ Supplier many-to-many relationship via GoodSupplier junction
    /// </summary>
    public static readonly IRelationshipMetadata<Good, Supplier> GoodSuppliers =
        new RelationshipMetadata<Good, Supplier>(
            type: RelationshipType.ManyToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(Good),
            relatedEntityName: nameof(Supplier),
            displayName: "Good Suppliers",
            junctionEntityName: nameof(GoodSupplier),
            navigationPropertyName: nameof(Good.GoodSuppliers),
            description: "Manages supplier relationships for goods with pricing and lead time metadata");

    /// <summary>
    /// Category → Good one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<Category, Good> CategoryGoods =
        new RelationshipMetadata<Category, Good>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(Category),
            relatedEntityName: nameof(Good),
            displayName: "Category Goods",
            navigationPropertyName: nameof(Category.Goods),
            description: "Goods belonging to a category");

    /// <summary>
    /// Category → Category self-referencing relationship (parent/child)
    /// </summary>
    public static readonly IRelationshipMetadata<Category, Category> CategorySubCategories =
        new RelationshipMetadata<Category, Category>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Optional,
            entityName: nameof(Category),
            relatedEntityName: nameof(Category),
            displayName: "Category Hierarchy",
            navigationPropertyName: nameof(Category.SubCategories),
            description: "Parent-child category relationships");

    /// <summary>
    /// Warehouse → StockLocation one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<Warehouse, StockLocation> WarehouseStockLocations =
        new RelationshipMetadata<Warehouse, StockLocation>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(Warehouse),
            relatedEntityName: nameof(StockLocation),
            displayName: "Warehouse Stock Locations",
            navigationPropertyName: nameof(Warehouse.StockLocations),
            description: "Stock locations within a warehouse");

    /// <summary>
    /// StockLocation → StockItem one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<StockLocation, StockItem> StockLocationItems =
        new RelationshipMetadata<StockLocation, StockItem>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(StockLocation),
            relatedEntityName: nameof(StockItem),
            displayName: "Stock Location Items",
            navigationPropertyName: nameof(StockLocation.StockItems),
            description: "Stock items stored at a location");

    /// <summary>
    /// Good → StockItem one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<Good, StockItem> GoodStockItems =
        new RelationshipMetadata<Good, StockItem>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(Good),
            relatedEntityName: nameof(StockItem),
            displayName: "Good Stock Items",
            navigationPropertyName: nameof(Good.StockItems),
            description: "Stock items for a specific good");

    /// <summary>
    /// Supplier → PurchaseOrder one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<Supplier, PurchaseOrder> SupplierPurchaseOrders =
        new RelationshipMetadata<Supplier, PurchaseOrder>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(Supplier),
            relatedEntityName: nameof(PurchaseOrder),
            displayName: "Supplier Purchase Orders",
            navigationPropertyName: nameof(Supplier.PurchaseOrders),
            description: "Purchase orders placed with a supplier");

    /// <summary>
    /// PurchaseOrder → PurchaseOrderItem one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<PurchaseOrder, PurchaseOrderItem> PurchaseOrderItems =
        new RelationshipMetadata<PurchaseOrder, PurchaseOrderItem>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(PurchaseOrder),
            relatedEntityName: nameof(PurchaseOrderItem),
            displayName: "Purchase Order Items",
            navigationPropertyName: nameof(PurchaseOrder.Items),
            description: "Line items in a purchase order");

    /// <summary>
    /// Good → PurchaseOrderItem one-to-many relationship
    /// </summary>
    public static readonly IRelationshipMetadata<Good, PurchaseOrderItem> GoodPurchaseOrderItems =
        new RelationshipMetadata<Good, PurchaseOrderItem>(
            type: RelationshipType.OneToMany,
            cardinality: RelationshipCardinality.Required,
            entityName: nameof(Good),
            relatedEntityName: nameof(PurchaseOrderItem),
            displayName: "Good Purchase Order Items",
            navigationPropertyName: nameof(Good.PurchaseOrderItems),
            description: "Purchase order items for a specific good");
}
