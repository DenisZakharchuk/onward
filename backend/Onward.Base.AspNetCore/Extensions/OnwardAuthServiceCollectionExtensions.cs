using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Onward.Base.Auth;

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

        services.AddAuthorization();

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

        services.AddAuthorization();

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
}
