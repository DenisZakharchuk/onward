using Inventorization.Base.DataAccess;
using Inventorization.Goods.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Goods.BL.EntityConfigurations;

public class GoodConfiguration : BaseEntityConfiguration<Good>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Good> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Sku)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(e => e.Sku)
            .IsUnique();
        
        builder.Property(e => e.UnitPrice)
            .HasPrecision(18, 2);
        
        builder.Property(e => e.QuantityInStock)
            .IsRequired()
            .HasDefaultValue(0);
        
        builder.Property(e => e.UnitOfMeasure)
            .HasMaxLength(50);
        
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
        
        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);
        
        // Relationship: Good belongs to Category (optional)
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Goods)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
