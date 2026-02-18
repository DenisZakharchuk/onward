using Inventorization.Goods.BL.Entities;

namespace Inventorization.Goods.BL.DbContexts;

/// <summary>
/// Database context for the Goods bounded context
/// </summary>
public class GoodsDbContext : DbContext
{
    public GoodsDbContext(DbContextOptions<GoodsDbContext> options) : base(options)
    {
    }
    
    public DbSet<Good> Goods => Set<Good>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<GoodSupplier> GoodSuppliers => Set<GoodSupplier>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoodsDbContext).Assembly);
    }
}
