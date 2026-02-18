using Inventorization.Base.Abstractions;
using Inventorization.Base.AspNetCore.Identity;
using Inventorization.Base.AspNetCore.Ownership;
using Inventorization.Base.Ownership;
using Microsoft.Extensions.DependencyInjection;

namespace Inventorization.Base.AspNetCore.Extensions;

/// <summary>
/// DI registration extensions for ownership-aware identity services.
/// Call one of these from each API project's <c>Program.cs</c> or DI extension class.
/// </summary>
public static class OwnershipServiceCollectionExtensions
{
    /// <summary>
    /// Registers ownership-aware identity services using a custom
    /// <typeparamref name="TFactory"/> to construct the ownership VO.
    /// </summary>
    /// <typeparam name="TOwnership">Concrete ownership VO.</typeparam>
    /// <typeparam name="TFactory">
    /// Concrete <see cref="IOwnershipFactory{TOwnership}"/> implementation.
    /// </typeparam>
    public static IServiceCollection AddOwnershipServices<TOwnership, TFactory>(
        this IServiceCollection services)
        where TOwnership : OwnershipValueObject
        where TFactory : class, IOwnershipFactory<TOwnership>
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IOwnershipFactory<TOwnership>, TFactory>();
        services.AddScoped<ICurrentIdentityContext<TOwnership>, HttpContextCurrentIdentityContext<TOwnership>>();
        services.AddScoped<ICurrentUserService<TOwnership>, ClaimsCurrentUserService<TOwnership>>();
        return services;
    }

    /// <summary>
    /// Convenience overload for the most common case: <see cref="UserTenantOwnership"/>
    /// with <see cref="UserTenantOwnershipFactory"/>.
    /// </summary>
    public static IServiceCollection AddUserTenantOwnershipServices(this IServiceCollection services)
        => services.AddOwnershipServices<UserTenantOwnership, UserTenantOwnershipFactory>();

    /// <summary>
    /// Convenience overload for single-user ownership: <see cref="UserOwnership"/>
    /// with <see cref="UserOwnershipFactory"/>.
    /// </summary>
    public static IServiceCollection AddUserOwnershipServices(this IServiceCollection services)
        => services.AddOwnershipServices<UserOwnership, UserOwnershipFactory>();
}
