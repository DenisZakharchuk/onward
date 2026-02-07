using Inventorization.Base.DataAccess;
using Inventorization.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Auth.Domain.EntityConfigurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(e => e.Email)
            .IsUnique();
        
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.IsActive)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);
    }
}
