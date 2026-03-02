using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Grpc.Net.Client;
using Onward.Base.Auth;
using Onward.Base.AspNetCore.Auth;
using Onward.Base.AspNetCore.Authorization;
using GrpcProto = Onward.Base.AspNetCore.GrpcProto;

namespace Onward.Base.AspNetCore.Extensions;

/// <summary>
/// Service-collection extensions for Onward JWT/anonymous authentication.
/// </summary>
public static class OnwardAuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers JWT bearer authentication + authorization by reading
    /// <see cref="OnwardJwtSettings"/> from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">
    /// The configuration section that holds <see cref="OnwardJwtSettings"/>.
    /// Defaults to <c>"JwtSettings"</c>.
    /// </param>
    public static IServiceCollection AddOnwardJwtAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = OnwardJwtSettings.SectionName)
    {
        var settings = configuration.GetSection(sectionName).Get<OnwardJwtSettings>()
            ?? throw new InvalidOperationException(
                $"JWT settings section '{sectionName}' is missing or empty.");

        services.Configure<OnwardJwtSettings>(configuration.GetSection(sectionName));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.SecretKey));

        services
            .AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey  = true,
                    ValidIssuer              = settings.Issuer,
                    ValidAudience            = settings.Audience,
                    IssuerSigningKey         = signingKey,
                    ClockSkew                = TimeSpan.Zero
                };
            });

        services.AddOnwardPermissionAuthorization();

        return services;
    }

    /// <summary>
    /// Registers JWT bearer authentication + authorization using pre-built
    /// <paramref name="tokenValidationParameters"/>.
    /// </summary>
    public static IServiceCollection AddOnwardJwtAuth(
        this IServiceCollection services,
        TokenValidationParameters tokenValidationParameters)
    {
        services
            .AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts => opts.TokenValidationParameters = tokenValidationParameters);

        services.AddOnwardPermissionAuthorization();

        return services;
    }

    /// <summary>
    /// Registers authentication + authorization without any bearer scheme
    /// (for services that are fully anonymous or gated upstream).
    /// </summary>
    public static IServiceCollection AddOnwardAnonymousAuth(
        this IServiceCollection services)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Registers JWT bearer authentication + real-time introspection against the Auth Service
    /// (<see cref="AuthMode.Online"/>).
    /// <para>
    /// This is a superset of <see cref="AddOnwardJwtAuth(IServiceCollection, IConfiguration, string)"/>:
    /// the JWT signature and lifetime are still validated locally first; the introspection call
    /// happens inside <c>OnTokenValidated</c> and can reject already-revoked tokens or blocked users.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="jwtSectionName">JWT settings section name. Defaults to <c>"JwtSettings"</c>.</param>
    /// <param name="onlineSectionName">Online auth settings section name. Defaults to <c>"OnlineAuth"</c>.</param>
    /// <param name="configureHttpClient">
    /// Optional delegate to further configure the <see cref="HttpClient"/> used by
    /// <see cref="HttpAuthIntrospectionClient"/> (e.g. add Polly retry policies, timeouts).
    /// </param>
    public static IServiceCollection AddOnwardOnlineAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        string jwtSectionName = OnwardJwtSettings.SectionName,
        string onlineSectionName = OnwardOnlineAuthSettings.SectionName,
        Action<IHttpClientBuilder>? configureHttpClient = null)
    {
        // ── Read settings ──────────────────────────────────────────────────
        var onlineSettings = configuration.GetSection(onlineSectionName).Get<OnwardOnlineAuthSettings>()
            ?? throw new InvalidOperationException(
                $"Online auth settings section '{onlineSectionName}' is missing or empty.");

        services.Configure<OnwardOnlineAuthSettings>(configuration.GetSection(onlineSectionName));

        // ── Base JWT auth (signature + lifetime) ───────────────────────────
        var jwtSettings = configuration.GetSection(jwtSectionName).Get<OnwardJwtSettings>()
            ?? throw new InvalidOperationException(
                $"JWT settings section '{jwtSectionName}' is missing or empty.");

        services.Configure<OnwardJwtSettings>(configuration.GetSection(jwtSectionName));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        // ── In-process cache ───────────────────────────────────────────────
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        // ── Introspection transport ────────────────────────────────────────
        var useGrpc = onlineSettings.Transport.Equals("Grpc", StringComparison.OrdinalIgnoreCase);

        if (useGrpc)
        {
            // gRPC channel + generated stub client
            services.AddSingleton(sp =>
            {
                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(
                    onlineSettings.AuthServiceBaseUrl.TrimEnd('/'));
                return new GrpcProto.AuthIntrospection.AuthIntrospectionClient(channel);
            });

            // Wrap with the caching decorator via the gRPC adapter
            services.AddHttpContextAccessor();
            services.AddScoped<GrpcAuthIntrospectionClient>();
            services.AddScoped<IAuthIntrospectionClient>(sp =>
                new CachedAuthIntrospectionClient(
                    sp.GetRequiredService<GrpcAuthIntrospectionClient>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OnwardOnlineAuthSettings>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedAuthIntrospectionClient>>()));
        }
        else
        {
            // HTTP typed client
            var builder = services
                .AddHttpClient<HttpAuthIntrospectionClient>(client =>
                {
                    client.BaseAddress = new Uri(onlineSettings.AuthServiceBaseUrl.TrimEnd('/') + "/");
                    client.Timeout = TimeSpan.FromSeconds(onlineSettings.TimeoutSeconds);
                });

            configureHttpClient?.Invoke(builder);

            // Wrap with the caching decorator
            services.AddScoped<IAuthIntrospectionClient>(sp =>
                new CachedAuthIntrospectionClient(
                    sp.GetRequiredService<HttpAuthIntrospectionClient>(),
                    sp.GetRequiredService<IMemoryCache>(),
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OnwardOnlineAuthSettings>>(),
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CachedAuthIntrospectionClient>>()));
        }

        // ── Online events handler ──────────────────────────────────────────
        services.AddScoped<OnwardOnlineJwtBearerEventsHandler>();

        // ── Authentication pipeline ────────────────────────────────────────
        services
            .AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey  = true,
                    ValidIssuer              = jwtSettings.Issuer,
                    ValidAudience            = jwtSettings.Audience,
                    IssuerSigningKey         = signingKey,
                    ClockSkew                = TimeSpan.Zero
                };

                opts.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        // Resolve the scoped handler from DI on every request
                        var handler = context.HttpContext.RequestServices
                            .GetRequiredService<OnwardOnlineJwtBearerEventsHandler>();
                        return handler.OnTokenValidated(context);
                    }
                };
            });

        services.AddOnwardPermissionAuthorization();

        return services;
    }

    // ── Internal helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Registers the Onward dynamic permission policy provider and its handler.
    /// Called automatically by all auth registration methods that enable authorization.
    /// </summary>
    private static IServiceCollection AddOnwardPermissionAuthorization(this IServiceCollection services)
    {
        // Dynamic provider — resolves "Resource.Action" policy names at runtime
        services.AddSingleton<IAuthorizationPolicyProvider, OnwardPermissionPolicyProvider>();

        // Handler that evaluates OnwardPermissionRequirement against claims
        services.AddScoped<IAuthorizationHandler, OnwardPermissionAuthorizationHandler>();

        services.AddAuthorization();

        return services;
    }
}
