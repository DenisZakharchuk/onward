namespace Onward.Base.Auth;

/// <summary>
/// Strongly-typed JWT configuration settings.
/// Bind from the "JwtSettings" configuration section.
/// </summary>
public sealed class OnwardJwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>Signing key used to sign and validate tokens.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>JWT issuer claim value (iss).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>JWT audience claim value (aud).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access token lifetime in minutes. Default: 15.</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>Refresh token lifetime in days. Default: 7.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
