using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Inventorization.Base.DataAccess;

/// <summary>
/// EF Core value converter for Enumeration types.
/// Stores the integer value in database, uses type-safe Smart Enum in code.
/// </summary>
public class EnumerationConverter<TEnumeration> : ValueConverter<TEnumeration, int>
    where TEnumeration : Models.Enumeration
{
    public EnumerationConverter()
        : base(
            enumeration => enumeration.Value,
            value => Models.Enumeration.FromValueOrThrow<TEnumeration>(value))
    {
    }
}
