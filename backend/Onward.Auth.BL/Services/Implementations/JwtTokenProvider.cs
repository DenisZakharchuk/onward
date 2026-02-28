using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Onward.Auth.BL.Entities;
using Onward.Base.Auth;

namespace Onward.Auth.BL.Services.Implementations;

/// <summary>
/// JWT token provider implementation using strongly-typed <see cref="OnwardJwtSettings"/>.
/// </summary>
public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly OnwardJwtSettings _settings;
    private readonly ILogger<JwtTokenProvider> _logger;

    public JwtTokenProvider(IOptions<OnwardJwtSettings> options, ILogger<JwtTokenProvider> logger)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public string CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        try
        {
            var key = BuildSigningKey();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new("name", user.FullName)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (permissions.Any())
                claims.Add(new Claim("permissions", string.Join(",", permissions)));

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating access token for user {UserId}", user.Id);
            throw;
        }
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = BuildSigningKey();
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }

    private SymmetricSecurityKey BuildSigningKey()
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }
}

/// <summary>
/// Interface for JWT token operations
/// </summary>
public interface IJwtTokenProvider
{
    string CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string CreateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
