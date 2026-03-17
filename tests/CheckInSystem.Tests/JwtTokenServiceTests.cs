using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace CheckInSystem.Tests;

public sealed class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _service = new JwtTokenService(Options.Create(new JwtSettings
        {
            Issuer = "CheckInSystem.Tests",
            Audience = "CheckInSystem.Tests.Client",
            SecretKey = "THIS_IS_A_TEST_SECRET_KEY_THAT_IS_LONG_ENOUGH_12345",
            AdminTokenExpiryMinutes = 120,
            CheckInTokenExpiryHours = 24
        }));
    }

    [Fact]
    public void GenerateAdminToken_ShouldIncludeRolesAndDistinctPermissions()
    {
        var user = CreateUserWithRoles();

        var (token, expiresAtUtc) = _service.GenerateAdminToken(user);
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.True(expiresAtUtc > DateTime.UtcNow);
        Assert.Contains(jwtToken.Claims, x => x.Type == ClaimTypes.Role && x.Value == "Administrator");
        Assert.Contains(jwtToken.Claims, x => x.Type == ClaimTypes.Role && x.Value == "Auditor");

        var permissions = jwtToken.Claims.Where(x => x.Type == "permission").Select(x => x.Value).ToArray();
        Assert.Equal(2, permissions.Length);
        Assert.Contains("users.read", permissions);
        Assert.Contains("users.write", permissions);
        Assert.Contains(jwtToken.Claims, x => x.Type == "token_type" && x.Value == "admin");
    }

    [Fact]
    public void GenerateCheckInToken_ShouldValidateSuccessfully()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice"
        };

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        var (token, tokenId, issuedAtUtc, returnedExpiresAtUtc) = _service.GenerateCheckInToken(user, expiresAtUtc);

        var principal = _service.ValidateCheckInToken(token);

        Assert.Equal(expiresAtUtc, returnedExpiresAtUtc);
        Assert.True(issuedAtUtc <= returnedExpiresAtUtc);
        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("checkin", principal.FindFirstValue("token_type"));
        Assert.Equal(tokenId, _service.ReadIdentifiers(token).TokenId);
    }

    [Fact]
    public void ReadIdentifiers_ShouldReturnUserIdAndTokenId()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "alice"
        };

        var (token, tokenId, _, _) = _service.GenerateCheckInToken(user, DateTime.UtcNow.AddHours(1));

        var (userId, parsedTokenId) = _service.ReadIdentifiers(token);

        Assert.Equal(user.Id, userId);
        Assert.Equal(tokenId, parsedTokenId);
    }

    private static User CreateUserWithRoles()
    {
        var readPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Users Read",
            Code = "users.read",
            Description = "Read users"
        };

        var writePermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Users Write",
            Code = "users.write",
            Description = "Write users"
        };

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Administrator"
        };

        adminRole.RolePermissions =
        [
            new RolePermission
            {
                Role = adminRole,
                RoleId = adminRole.Id,
                Permission = readPermission,
                PermissionId = readPermission.Id
            },
            new RolePermission
            {
                Role = adminRole,
                RoleId = adminRole.Id,
                Permission = writePermission,
                PermissionId = writePermission.Id
            }
        ];

        var auditorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Auditor"
        };

        auditorRole.RolePermissions =
        [
            new RolePermission
            {
                Role = auditorRole,
                RoleId = auditorRole.Id,
                Permission = readPermission,
                PermissionId = readPermission.Id
            }
        ];

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin"
        };

        user.UserRoles =
        [
            new UserRole
            {
                User = user,
                UserId = user.Id,
                Role = adminRole,
                RoleId = adminRole.Id
            },
            new UserRole
            {
                User = user,
                UserId = user.Id,
                Role = auditorRole,
                RoleId = auditorRole.Id
            }
        ];

        return user;
    }
}
