using Inventorization.Base.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventorization.Base.Models;

/// <summary>
/// Extension methods for applying IDataModelMetadata to Entity Framework configurations
/// </summary>
public static class DataModelMetadataExtensions
{
    /// <summary>
    /// Apply metadata-driven configuration to an EF Core entity
    /// </summary>
    public static EntityTypeBuilder<TEntity> ApplyMetadata<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        IDataModelMetadata<TEntity> metadata) 
        where TEntity : class
    {
        // Table configuration
        if (!string.IsNullOrEmpty(metadata.Schema))
            builder.ToTable(metadata.TableName, metadata.Schema);
        else
            builder.ToTable(metadata.TableName);

        // Primary key
        if (metadata.PrimaryKey.Any())
        {
            builder.HasKey(metadata.PrimaryKey.ToArray());
        }

        // Property configurations
        foreach (var property in metadata.Properties.Values)
        {
            ConfigureProperty(builder, property);
        }

        // Indexes
        foreach (var indexProps in metadata.Indexes)
        {
            builder.HasIndex(indexProps);
        }

        // Unique constraints
        foreach (var uniqueProps in metadata.UniqueConstraints)
        {
            builder.HasIndex(uniqueProps).IsUnique();
        }

        return builder;
    }

    /// <summary>
    /// Configure a single property based on metadata
    /// </summary>
    private static void ConfigureProperty<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        IDataPropertyMetadata propertyMetadata) 
        where TEntity : class
    {
        var propertyBuilder = builder.Property(propertyMetadata.PropertyName);

        // Required/Optional
        if (propertyMetadata.IsRequired)
            propertyBuilder.IsRequired();

        // Max length
        if (propertyMetadata.MaxLength.HasValue)
            propertyBuilder.HasMaxLength(propertyMetadata.MaxLength.Value);

        // Column type
        if (!string.IsNullOrEmpty(propertyMetadata.ColumnType))
            propertyBuilder.HasColumnType(propertyMetadata.ColumnType);

        // Column name
        if (!string.IsNullOrEmpty(propertyMetadata.ColumnName))
            propertyBuilder.HasColumnName(propertyMetadata.ColumnName);

        // Precision/Scale (for decimals)
        if (propertyMetadata.Precision.HasValue && propertyMetadata.Scale.HasValue)
            propertyBuilder.HasPrecision(propertyMetadata.Precision.Value, propertyMetadata.Scale.Value);

        // Default value
        if (propertyMetadata.DefaultValue != null)
            propertyBuilder.HasDefaultValue(propertyMetadata.DefaultValue);

        // Default SQL
        if (!string.IsNullOrEmpty(propertyMetadata.DefaultValueSql))
            propertyBuilder.HasDefaultValueSql(propertyMetadata.DefaultValueSql);

        // Computed
        if (propertyMetadata.IsComputed)
            propertyBuilder.ValueGeneratedOnAddOrUpdate();
    }

    /// <summary>
    /// Validate a DTO against entity metadata
    /// </summary>
    public static MetadataValidationResult ValidateAgainstMetadata<TEntity>(
        this IDataModelMetadata<TEntity> metadata,
        object dto,
        params string[] propertiesToValidate)
        where TEntity : class
    {
        var errors = new List<string>();
        var dtoType = dto.GetType();

        var propsToCheck = propertiesToValidate.Any() 
            ? metadata.Properties.Where(p => propertiesToValidate.Contains(p.Key))
            : metadata.Properties;

        foreach (var (propName, propMetadata) in propsToCheck)
        {
            var dtoProp = dtoType.GetProperty(propName);
            if (dtoProp == null) continue;

            var value = dtoProp.GetValue(dto);

            // Required validation
            if (propMetadata.IsRequired && value == null)
            {
                errors.Add(propMetadata.ValidationMessage 
                    ?? $"{propMetadata.DisplayName} is required");
                continue;
            }

            if (value == null) continue;

            // String validations
            if (propMetadata.PropertyType == typeof(string))
            {
                var stringValue = value.ToString() ?? string.Empty;

                // Max length
                if (propMetadata.MaxLength.HasValue && stringValue.Length > propMetadata.MaxLength.Value)
                {
                    errors.Add($"{propMetadata.DisplayName} must not exceed {propMetadata.MaxLength.Value} characters");
                }

                // Regex pattern
                if (!string.IsNullOrEmpty(propMetadata.RegexPattern))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(stringValue, propMetadata.RegexPattern))
                    {
                        errors.Add(propMetadata.ValidationMessage 
                            ?? $"{propMetadata.DisplayName} has invalid format");
                    }
                }
            }

            // Numeric validations
            if (IsNumericType(propMetadata.PropertyType))
            {
                var numericValue = Convert.ToDecimal(value);

                if (propMetadata.MinValue != null)
                {
                    var minValue = Convert.ToDecimal(propMetadata.MinValue);
                    if (numericValue < minValue)
                    {
                        errors.Add($"{propMetadata.DisplayName} must be at least {minValue}");
                    }
                }

                if (propMetadata.MaxValue != null)
                {
                    var maxValue = Convert.ToDecimal(propMetadata.MaxValue);
                    if (numericValue > maxValue)
                    {
                        errors.Add($"{propMetadata.DisplayName} must not exceed {maxValue}");
                    }
                }
            }
        }

        return new MetadataValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    private static bool IsNumericType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(int) 
            || underlyingType == typeof(long) 
            || underlyingType == typeof(decimal) 
            || underlyingType == typeof(double) 
            || underlyingType == typeof(float)
            || underlyingType == typeof(short)
            || underlyingType == typeof(byte);
    }
}

/// <summary>
/// Result of metadata-based validation
/// </summary>
public class MetadataValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
