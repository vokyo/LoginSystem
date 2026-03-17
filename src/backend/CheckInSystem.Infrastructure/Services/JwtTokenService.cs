using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CheckInSystem.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public (string Token, DateTime ExpiresAtUtc) GenerateAdminToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AdminTokenExpiryMinutes);
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("token_type", "admin")
        };

        claims.AddRange(user.UserRoles.Select(x => new Claim(ClaimTypes.Role, x.Role.Name)));
        claims.AddRange(permissions.Select(x => new Claim("permission", x)));

        return (BuildToken(claims, expiresAt), expiresAt);
    }

    public (string Token, string TokenId, DateTime IssuedAtUtc, DateTime ExpiresAtUtc) GenerateCheckInToken(User user, DateTime expiresAtUtc)
    {
        var issuedAt = DateTime.UtcNow;
        var tokenId = Guid.NewGuid().ToString("N");
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new("token_type", "checkin")
        };

        return (BuildToken(claims, expiresAtUtc, issuedAt), tokenId, issuedAt, expiresAtUtc);
    }

    public ClaimsPrincipal ValidateCheckInToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, BuildValidationParameters(validateLifetime: true), out _);
    }

    public (Guid? UserId, string? TokenId) ReadIdentifiers(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Guid? userId = Guid.TryParse(jwtToken.Claims.FirstOrDefault(x =>
                x.Type == JwtRegisteredClaimNames.Sub || x.Type == ClaimTypes.NameIdentifier)?.Value,
            out var parsedUserId)
            ? parsedUserId
            : null;

        var tokenId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
        return (userId, tokenId);
    }

    private string BuildToken(IEnumerable<Claim> claims, DateTime expiresAtUtc, DateTime? issuedAtUtc = null)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)), SecurityAlgorithms.HmacSha256);
        var jwtToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: issuedAtUtc ?? DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private TokenValidationParameters BuildValidationParameters(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = validateLifetime,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    }
}
