namespace Onward.Base.Models;

/// <summary>
/// Stores a point in time together with the original UTC offset, preserving offset fidelity
/// that <see cref="DateTimeOffset"/> loses when persisted via Npgsql's timestamptz.
/// Persisted as two columns via EF Core OwnsOne: {Property}_UtcTicks (bigint) and
/// {Property}_OffsetMinutes (smallint).
/// </summary>
public sealed class DateTimeWithOffset : IValueObject
{
    /// <summary>UTC ticks (100-nanosecond intervals since 0001-01-01T00:00:00).</summary>
    public long UtcTicks { get; private set; }

    /// <summary>Original UTC offset in whole minutes (e.g. +180 for UTC+3).</summary>
    public short OffsetMinutes { get; private set; }

    public DateTimeWithOffset(long utcTicks, short offsetMinutes)
    {
        UtcTicks = utcTicks;
        OffsetMinutes = offsetMinutes;
    }

    /// <summary>Private parameterless constructor required by EF Core.</summary>
    private DateTimeWithOffset() { }

    /// <summary>Creates a <see cref="DateTimeWithOffset"/> from a <see cref="DateTimeOffset"/>,
    /// preserving the original offset.</summary>
    public static DateTimeWithOffset From(DateTimeOffset dto) =>
        new(dto.UtcTicks, (short)dto.Offset.TotalMinutes);

    /// <summary>Reconstructs the <see cref="DateTimeOffset"/> with the original offset.</summary>
    public DateTimeOffset ToDateTimeOffset() =>
        new DateTimeOffset(UtcTicks, TimeSpan.FromMinutes(OffsetMinutes));

    /// <summary>Returns the ISO 8601 round-trip string, e.g. "2026-04-07T12:00:00.0000000+03:00".</summary>
    public string ToIso8601() => ToDateTimeOffset().ToString("o");
}
