using Microsoft.AspNetCore.Builder;

namespace Onward.Base.AspNetCore.Extensions;

/// <summary>
/// Application builder extensions for the Onward authentication middleware pipeline.
/// </summary>
public static class OnwardAuthMiddlewareExtensions
{
    /// <summary>
    /// Adds <c>UseAuthentication()</c> followed by <c>UseAuthorization()</c>
    /// in the correct order required by ASP.NET Core.
    /// </summary>
    public static IApplicationBuilder UseOnwardAuth(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}
