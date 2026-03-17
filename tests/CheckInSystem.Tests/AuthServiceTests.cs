using CheckInSystem.Application.DTOs.Auth;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CheckInSystem.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ShouldReturnTokenAndDistinctPermissions_WhenCredentialsAreValid()
    {
        var user = CreateAuthenticatedUser();
        var expiresAtUtc = DateTime.UtcNow.AddHours(2);

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(x => x.GenerateAdminToken(user))
            .Returns(("admin-token", expiresAtUtc));

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<AuthService>.Instance);

        var result = await service.LoginAsync(new LoginRequestDto
        {
            UserName = user.UserName,
            Password = "Admin123!"
        });

        Assert.Equal("admin-token", result.Token);
        Assert.Equal(expiresAtUtc, result.ExpiresAtUtc);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(2, result.Roles.Count);
        Assert.Contains("Administrator", result.Roles);
        Assert.Contains("Auditor", result.Roles);
        Assert.Equal(2, result.Permissions.Count);
        Assert.Contains("users.read", result.Permissions);
        Assert.Contains("users.write", result.Permissions);
        Assert.True(user.LastLoginAtUtc.HasValue);

        userRepository.Verify(x => x.Update(user), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        jwtTokenService.Verify(x => x.GenerateAdminToken(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        var user = CreateAuthenticatedUser();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = new Mock<IJwtTokenService>();

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<AuthService>.Instance);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequestDto
        {
            UserName = user.UserName,
            Password = "WrongPassword!"
        }));

        Assert.Equal("Invalid username or password.", exception.Message);
        userRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        jwtTokenService.Verify(x => x.GenerateAdminToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserIsDisabled()
    {
        var user = CreateAuthenticatedUser();
        user.IsActive = false;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = new Mock<IJwtTokenService>();

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object,
            NullLogger<AuthService>.Instance);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync(new LoginRequestDto
        {
            UserName = user.UserName,
            Password = "Admin123!"
        }));

        Assert.Equal("The user is disabled.", exception.Message);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        jwtTokenService.Verify(x => x.GenerateAdminToken(It.IsAny<User>()), Times.Never);
    }

    private static User CreateAuthenticatedUser()
    {
        var readPermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Read Users",
            Code = "users.read",
            Description = "Read users"
        };

        var writePermission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Write Users",
            Code = "users.write",
            Description = "Write users"
        };

        var administratorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Administrator",
            Description = "Administrators"
        };

        administratorRole.RolePermissions =
        [
            new RolePermission
            {
                Role = administratorRole,
                RoleId = administratorRole.Id,
                Permission = readPermission,
                PermissionId = readPermission.Id
            },
            new RolePermission
            {
                Role = administratorRole,
                RoleId = administratorRole.Id,
                Permission = writePermission,
                PermissionId = writePermission.Id
            }
        ];

        var auditorRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Auditor",
            Description = "Auditors"
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
            UserName = "admin",
            FullName = "Admin User",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsActive = true
        };

        user.UserRoles =
        [
            new UserRole
            {
                User = user,
                UserId = user.Id,
                Role = administratorRole,
                RoleId = administratorRole.Id
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
