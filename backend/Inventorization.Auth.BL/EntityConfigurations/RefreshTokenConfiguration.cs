using Inventorization.Base.DataAccess;
using Inventorization.Auth.BL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Auth.BL.EntityConfigurations;

public class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.HasIndex(e => e.Token)
            .IsUnique();
        
        builder.Property(e => e.ExpiryDate)
            .IsRequired();
        
        builder.Property(e => e.RevokedAt)
            .IsRequired(false);
        
        builder.Property(e => e.Family)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasIndex(e => e.Family);
        
        builder.Property(e => e.RotationCount)
            .IsRequired();
        
        builder.Property(e => e.IpAddress)
            .IsRequired()
            .HasMaxLength(45); // IPv6 max length
        
        builder.Property(e => e.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        // Relationship: RefreshToken belongs to User (required) - CASCADE DELETE PROHIBITED
        builder.HasOne(e => e.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Self-referencing relationship: ReplacedByToken
        builder.HasOne(e => e.ReplacedByToken)
            .WithMany()
            .HasForeignKey(e => e.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
