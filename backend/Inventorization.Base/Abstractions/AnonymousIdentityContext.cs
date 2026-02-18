using Inventorization.Base.Ownership;

namespace Inventorization.Base.Abstractions;

/// <summary>
/// Null-object implementation of <see cref="ICurrentIdentityContext{TOwnership}"/>.
/// Represents an unauthenticated (anonymous) caller with no ownership, no roles,
/// and no email.
/// Register this as the default when no real identity is available:
/// background jobs, integration test harnesses, or migration tooling.
/// </summary>
/// <typeparam name="TOwnership">
/// Concrete <see cref="OwnershipValueObject"/> for this bounded context.
/// </typeparam>
public sealed class AnonymousIdentityContext<TOwnership> : ICurrentIdentityContext<TOwnership>
    where TOwnership : OwnershipValueObject
{
    /// <summary>Shared singleton instance. Stateless — safe to reuse.</summary>
    public static readonly AnonymousIdentityContext<TOwnership> Instance = new();

    /// <inheritdoc />
    public TOwnership? Ownership => null;

    /// <inheritdoc />
    public string? Email => null;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => Array.Empty<string>();

    /// <inheritdoc />
    public bool IsAuthenticated => false;

    /// <inheritdoc />
    public bool IsInRole(string role) => false;
}
