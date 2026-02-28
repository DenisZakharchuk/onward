using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Onward.Base.AspNetCore.Extensions;

/// <summary>
/// Swagger/OpenAPI extensions for Onward security definitions.
/// </summary>
public static class OnwardSwaggerExtensions
{
    private const string BearerSchemeId = "Bearer";

    /// <summary>
    /// Adds a JWT Bearer <c>SecurityDefinition</c> and a global
    /// <c>SecurityRequirement</c> to <paramref name="c"/> so that
    /// Swagger UI shows the "Authorize" button for every endpoint.
    /// Call this inside your <c>AddSwaggerGen(c => ...)</c> lambda.
    /// </summary>
    public static void AddOnwardJwtSecurity(this SwaggerGenOptions c)
    {
        c.AddSecurityDefinition(BearerSchemeId, new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. " +
                          "Enter 'Bearer' [space] and then your token in the text input below. " +
                          "Example: 'Bearer eyJhbGci...'",
            Name   = "Authorization",
            In     = ParameterLocation.Header,
            Type   = SecuritySchemeType.ApiKey,
            Scheme = BearerSchemeId
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = BearerSchemeId
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}

