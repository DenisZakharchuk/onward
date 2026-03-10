using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onward.Base.Models;
using Onward.Base.Ownership;

namespace Onward.Base.DataAccess;

/// <summary>
/// Base entity configuration for entities inheriting from BaseEntity&lt;TKey&gt;.
/// Handles common configuration like primary key, table naming, and common indexes.
/// </summary>
/// <typeparam name="TEntity">Entity type (must inherit from BaseEntity&lt;TKey&gt;)</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public abstract class BaseEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity<TKey>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // Table name: pluralize entity name
        var tableName = GetTableName();
        builder.ToTable(tableName);

        // Primary key (inherited from BaseEntity<TKey>)
        builder.HasKey(e => e.Id);

        // Call derived class configuration
        ConfigureEntity(builder);
    }

    /// <summary>
    /// Override this method to configure entity-specific properties and relationships
    /// </summary>
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);

    /// <summary>
    /// Gets the table name for the entity. Override to customize table naming.
    /// Default: simple pluralization (append 's', handle 'y' → 'ies', etc.)
    /// </summary>
    protected virtual string GetTableName()
    {
        var entityName = typeof(TEntity).Name;

        if (entityName.EndsWith("s") || entityName.EndsWith("x") || entityName.EndsWith("ch") || entityName.EndsWith("sh"))
            return entityName + "es";

        if (entityName.EndsWith("y") && !IsVowel(entityName[^2]))
            return entityName[..^1] + "ies";

        return entityName + "s";
    }

    private static bool IsVowel(char c) => "aeiouAEIOU".Contains(c);
}

/// <summary>
/// Base entity configuration for Guid-primary-key entities — convenience alias.
/// </summary>
/// <typeparam name="TEntity">Entity type (must inherit from BaseEntity&lt;Guid&gt;)</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity, Guid>
    where TEntity : BaseEntity<Guid>
{
}

/// <summary>
/// Base entity configuration for Guid-PK entities that extend OwnedBaseEntity&lt;TOwnership&gt;.
/// Automatically configures OwnsOne mappings for Ownership and LastModifiedOwnership.
/// </summary>
/// <typeparam name="TEntity">Entity type (must inherit from OwnedBaseEntity&lt;TOwnership, Guid&gt;)</typeparam>
/// <typeparam name="TOwnership">The ownership value object type</typeparam>
public abstract class OwnedBaseEntityConfiguration<TEntity, TOwnership>
    : BaseEntityConfiguration<TEntity>
    where TEntity : OwnedBaseEntity<TOwnership, Guid>
    where TOwnership : OwnershipValueObject
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);
        builder.OwnsOne(e => e.Ownership);
        builder.OwnsOne(e => e.LastModifiedOwnership);
    }
}

