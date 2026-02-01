using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Inventorization.Auth.Domain.Entities;

namespace Inventorization.Auth.Domain.Services.Implementations;

/// <summary>
/// JWT token provider implementation
/// </summary>
public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenProvider> _logger;

    public JwtTokenProvider(IConfiguration config, ILogger<JwtTokenProvider> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string CreateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        try
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new("name", user.FullName)
            };

            // Add roles
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Add permissions as comma-separated string
            if (permissions.Any())
                claims.Add(new Claim("permissions", string.Join(",", permissions)));

            var expiryMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
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
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Key not configured")));

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
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
