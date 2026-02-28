using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onward.Auth.BL.Entities;

namespace Onward.Auth.BL.EntityConfigurations;

public sealed class BlacklistedTokenConfiguration : IEntityTypeConfiguration<BlacklistedToken>
{
    public void Configure(EntityTypeBuilder<BlacklistedToken> builder)
    {
        builder.ToTable("BlacklistedTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Jti)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .IsRequired();

        // Fast exact lookup by jti — the primary query pattern
        builder.HasIndex(x => x.Jti)
            .IsUnique()
            .HasDatabaseName("IX_BlacklistedTokens_Jti");

        // Used by the cleanup job to purge expired entries efficiently
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_BlacklistedTokens_ExpiresAt");

        // Useful for user-level audit queries
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_BlacklistedTokens_UserId");
    }
}
