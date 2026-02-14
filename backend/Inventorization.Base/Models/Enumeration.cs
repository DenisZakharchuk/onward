using System.Reflection;

namespace Inventorization.Base.Models;

/// <summary>
/// Base class for creating type-safe enumerations (Smart Enums).
/// Provides enum-like behavior with string names, int values, and extensibility.
/// Stores as int in database, serializes as string in JSON.
/// </summary>
public abstract class Enumeration : IComparable<Enumeration>, IEquatable<Enumeration>
{
    public string Name { get; }
    public int Value { get; }

    protected Enumeration(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return fields
            .Where(f => f.FieldType == typeof(T))
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    public static T? FromValue<T>(int value) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(e => e.Value == value);
    }

    public static T? FromName<T>(string name) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static T FromNameOrThrow<T>(string name) where T : Enumeration
    {
        return FromName<T>(name) 
            ?? throw new ArgumentException($"'{name}' is not a valid {typeof(T).Name}");
    }

    public static T FromValueOrThrow<T>(int value) where T : Enumeration
    {
        return FromValue<T>(value) 
            ?? throw new ArgumentException($"{value} is not a valid {typeof(T).Name} value");
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public bool Equals(Enumeration? other)
    {
        if (other is null)
            return false;

        return GetType() == other.GetType() && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(Enumeration? other) => Value.CompareTo(other?.Value);

    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Enumeration? left, Enumeration? right) => !(left == right);

    public static bool operator <(Enumeration left, Enumeration right) => left.CompareTo(right) < 0;

    public static bool operator <=(Enumeration left, Enumeration right) => left.CompareTo(right) <= 0;

    public static bool operator >(Enumeration left, Enumeration right) => left.CompareTo(right) > 0;

    public static bool operator >=(Enumeration left, Enumeration right) => left.CompareTo(right) >= 0;

    // Implicit conversion to string (for JSON/API)
    public static implicit operator string(Enumeration enumeration) => enumeration.Name;

    // Implicit conversion to int (for database)
    public static implicit operator int(Enumeration enumeration) => enumeration.Value;
}
