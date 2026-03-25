namespace Zivro.Infrastructure.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zivro.Application.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;

    /// <summary>
    /// Initializes a new instance of JwtTokenService.
    /// </summary>
    /// <param name="configuration">Application configuration for JWT settings.</param>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _secretKey = _configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey not configured");
        _issuer = _configuration["JWT:Issuer"] ?? "Zivro.API";
        _audience = _configuration["JWT:Audience"] ?? "Zivro.App";
        _accessTokenExpirationMinutes = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "15");
    }

    /// <summary>
    /// Generates a new access token with user claims.
    /// </summary>
    public string GenerateAccessToken(Guid userId, string email, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new("sub", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token and returns the ClaimsPrincipal if valid.
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the user ID from a valid JWT token.
    /// </summary>
    public Guid? ExtractUserId(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Extracts the user email from a valid JWT token.
    /// </summary>
    public string? ExtractEmail(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }
}
