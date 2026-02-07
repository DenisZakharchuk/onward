using Inventorization.Base.DataAccess;
using Inventorization.Goods.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.Domain.EntityConfigurations;

public class WarehouseConfiguration : BaseEntityConfiguration<Warehouse>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Warehouse> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => e.Code)
            .IsUnique();
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Address)
            .HasMaxLength(500);
        
        builder.Property(e => e.City)
            .HasMaxLength(100);
        
        builder.Property(e => e.Country)
            .HasMaxLength(100);
        
        builder.Property(e => e.PostalCode)
            .HasMaxLength(20);
        
        builder.Property(e => e.ManagerName)
            .HasMaxLength(200);
        
        builder.Property(e => e.ContactPhone)
            .HasMaxLength(50);
    }
}
