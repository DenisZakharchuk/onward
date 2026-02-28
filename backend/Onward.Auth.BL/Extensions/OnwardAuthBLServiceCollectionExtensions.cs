using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onward.Auth.BL.DataAccess.Repositories;
using Onward.Auth.BL.DataAccess.UnitOfWork;
using Onward.Auth.BL.DataServices;
using Onward.Auth.BL.DbContexts;
using Onward.Auth.BL.Entities;
using Onward.Auth.BL.Creators;
using Onward.Auth.BL.Mappers;
using Onward.Auth.BL.Modifiers;
using Onward.Auth.BL.PropertyAccessors;
using Onward.Auth.BL.SearchProviders;
using Onward.Auth.BL.Services.Abstractions;
using Onward.Auth.BL.Services.Implementations;
using Onward.Auth.BL.Validators;
using Onward.Auth.DTO.DTO.User;
using Onward.Auth.DTO.DTO.Role;
using Onward.Auth.DTO.DTO.Permission;
using Onward.Base.Abstractions;
using Onward.Base.DataAccess;
using Onward.Base.DTOs;
// Alias ambiguous service-abstractions interfaces so they never clash with
// DataAccess.Repositories or Onward.Base names.
using IUserRepo          = Onward.Auth.BL.Services.Abstractions.IUserRepository;
using IUserRepoDA        = Onward.Auth.BL.DataAccess.Repositories.IUserRepository;
using IRefreshTokenRepo  = Onward.Auth.BL.Services.Abstractions.IRefreshTokenRepository;
using IPasswordHasherSvc = Onward.Base.Abstractions.IPasswordHasher;

namespace Onward.Auth.BL.Extensions;

/// <summary>
/// DI registration extension for all Onward.Auth business-layer services.
/// Call <see cref="AddOnwardAuthBusinessServices"/> from every host that needs Auth BL.
/// </summary>
public static class OnwardAuthBLServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Auth DbContext, repositories, UoW, data services, mappers,
    /// validators, JWT provider, and domain-level auth services.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configureDb">EF Core DbContext options builder delegate.</param>
    public static IServiceCollection AddOnwardAuthBusinessServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDb)
    {
        // ===== DbContext =====
        services.AddDbContext<AuthDbContext>(configureDb);

        // ===== Repositories & UnitOfWork =====
        services.AddScoped<IUserRepo>(sp =>
            new UserRepository(sp.GetRequiredService<AuthDbContext>()));

        // IUserRepository (DataAccess.Repositories namespace) used by validators
        services.AddScoped<IUserRepoDA>(sp =>
            new UserRepository(sp.GetRequiredService<AuthDbContext>()));

        services.AddScoped<IRefreshTokenRepo>(sp =>
            new RefreshTokenRepository(sp.GetRequiredService<AuthDbContext>()));

        services.AddScoped<IRepository<Role>>(sp =>
            new BaseRepository<Role>(sp.GetRequiredService<AuthDbContext>()));
        services.AddScoped<IRepository<Permission>>(sp =>
            new BaseRepository<Permission>(sp.GetRequiredService<AuthDbContext>()));
        services.AddScoped<IRepository<User>>(sp =>
            new UserRepository(sp.GetRequiredService<AuthDbContext>()));
        services.AddScoped<IRepository<UserRole>>(sp =>
            new BaseRepository<UserRole>(sp.GetRequiredService<AuthDbContext>()));

        services.AddScoped<IAuthUnitOfWork, AuthUnitOfWork>();
        services.AddScoped<Onward.Base.DataAccess.IUnitOfWork>(sp =>
            sp.GetRequiredService<IAuthUnitOfWork>());

        // ===== User =====
        services.AddScoped<IMapper<User, UserDetailsDTO>, UserMapper>();
        services.AddScoped<IEntityCreator<User, CreateUserDTO>, UserCreator>();
        services.AddScoped<IEntityModifier<User, UpdateUserDTO>, UserModifier>();
        services.AddScoped<ISearchQueryProvider<User, UserSearchDTO>, UserSearchProvider>();
        services.AddScoped<IValidator<CreateUserDTO>, CreateUserDtoValidator>();
        services.AddScoped<IValidator<UpdateUserDTO>, UpdateUserDtoValidator>();
        services.AddScoped<IUserDataService, UserDataService>();

        // ===== Role =====
        services.AddScoped<IMapper<Role, RoleDetailsDTO>, RoleMapper>();
        services.AddScoped<IEntityCreator<Role, CreateRoleDTO>, RoleCreator>();
        services.AddScoped<IEntityModifier<Role, UpdateRoleDTO>, RoleModifier>();
        services.AddScoped<ISearchQueryProvider<Role, RoleSearchDTO>, RoleSearchProvider>();
        services.AddScoped<IValidator<CreateRoleDTO>, CreateRoleDtoValidator>();
        services.AddScoped<IValidator<UpdateRoleDTO>, UpdateRoleDtoValidator>();
        services.AddScoped<IRoleDataService, RoleDataService>();

        // ===== Permission =====
        services.AddScoped<IMapper<Permission, PermissionDetailsDTO>, PermissionMapper>();
        services.AddScoped<IEntityCreator<Permission, CreatePermissionDTO>, PermissionCreator>();
        services.AddScoped<IEntityModifier<Permission, UpdatePermissionDTO>, PermissionModifier>();
        services.AddScoped<ISearchQueryProvider<Permission, PermissionSearchDTO>, PermissionSearchProvider>();
        services.AddScoped<IValidator<CreatePermissionDTO>, CreatePermissionDtoValidator>();
        services.AddScoped<IValidator<UpdatePermissionDTO>, UpdatePermissionDtoValidator>();
        services.AddScoped<IPermissionDataService, PermissionDataService>();

        // ===== Password Hasher =====
        services.AddScoped<IPasswordHasherSvc, BcryptPasswordHasher>();

        // ===== Relationship Property Accessors =====
        services.AddScoped<IEntityIdPropertyAccessor<UserRole>, UserRoleEntityIdPropertyAccessor>();
        services.AddScoped<IRelatedEntityIdPropertyAccessor<UserRole>, UserRoleRelatedEntityIdPropertyAccessor>();
        services.AddScoped<IEntityIdPropertyAccessor<RolePermission>, RolePermissionEntityIdPropertyAccessor>();
        services.AddScoped<IRelatedEntityIdPropertyAccessor<RolePermission>, RolePermissionRelatedEntityIdPropertyAccessor>();

        // ===== Relationship Metadata & Managers =====
        services.AddSingleton<IRelationshipMetadata<User, Role>>(
            Onward.Auth.BL.DataModelRelationships.UserRoles);
        services.AddScoped<IRelationshipManager<User, Role>, UserRoleRelationshipManager>();
        services.AddScoped<IValidator<EntityReferencesDTO>, EntityReferencesValidator>();

        // ===== Domain Auth Services =====
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<ITokenRotationService, TokenRotationService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IOnwardAuthorizationService, AuthorizationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // ===== Online-auth: Blacklist, Introspection, User Admin =====
        services.AddScoped<ITokenBlacklist, PostgresTokenBlacklist>();
        services.AddScoped<ITokenIntrospectionService, TokenIntrospectionService>();
        services.AddScoped<IUserAdminService, UserAdminService>();

        return services;
    }
}
