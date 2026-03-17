using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.DTOs.Users;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CheckInSystem.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ShouldReturnMappedUsersAndTotalCount()
    {
        var role = CreateRole("Administrator");
        var users = new[]
        {
            CreateUser("alice", "Alice", "alice@example.com", role),
            CreateUser("bob", "Bob", "bob@example.com", role)
        };

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetPagedAsync("a", true, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        userRepository.Setup(x => x.CountAsync("a", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);

        var service = new UserService(
            userRepository.Object,
            Mock.Of<IRoleRepository>(),
            Mock.Of<IUnitOfWork>(),
            NullLogger<UserService>.Instance);

        var result = await service.GetPagedAsync(new UserListQueryDto
        {
            Search = "a",
            IsActive = true,
            PageNumber = 2,
            PageSize = 5
        });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(12, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Contains(result.Items, x => x.UserName == "alice");
        Assert.All(result.Items, x => Assert.Single(x.Roles));
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPasswordAssignRolesAndReturnCreatedUser()
    {
        var role = CreateRole("Administrator");
        var request = new CreateUserDto
        {
            UserName = "  alice  ",
            FullName = "  Alice Smith  ",
            Email = "  alice@example.com  ",
            Password = "Admin123!",
            IsActive = true,
            RoleIds = [role.Id]
        };

        User? addedUser = null;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.ExistsByUserNameAsync("alice", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository.Setup(x => x.ExistsByEmailAsync("alice@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => addedUser = user)
            .Returns(Task.CompletedTask);
        userRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                if (addedUser is null || addedUser.Id != id)
                {
                    return null;
                }

                return CreateLoadedUser(
                    addedUser.Id,
                    addedUser.UserName,
                    addedUser.FullName,
                    addedUser.Email,
                    addedUser.IsActive,
                    role);
            });

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { role });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            userRepository.Object,
            roleRepository.Object,
            unitOfWork.Object,
            NullLogger<UserService>.Instance);

        var result = await service.CreateAsync(request);

        Assert.NotNull(addedUser);
        Assert.Equal("alice", addedUser!.UserName);
        Assert.Equal("Alice Smith", addedUser.FullName);
        Assert.Equal("alice@example.com", addedUser.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("Admin123!", addedUser.PasswordHash));
        Assert.NotEqual("Admin123!", addedUser.PasswordHash);
        Assert.Single(addedUser.UserRoles);

        Assert.Equal("alice", result.UserName);
        Assert.Equal("Alice Smith", result.FullName);
        Assert.Single(result.Roles);
        Assert.Equal(role.Name, result.Roles.Single().Name);

        userRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenUserDataIsNotUniqueOrRolesMissing()
    {
        var requestedRoleIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.ExistsByUserNameAsync("alice", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        userRepository.Setup(x => x.ExistsByEmailAsync("alice@example.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateRole("Administrator", requestedRoleIds[0]) });

        var service = new UserService(
            userRepository.Object,
            roleRepository.Object,
            Mock.Of<IUnitOfWork>(),
            NullLogger<UserService>.Instance);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new CreateUserDto
        {
            UserName = "alice",
            FullName = "Alice Smith",
            Email = "alice@example.com",
            Password = "Admin123!",
            RoleIds = requestedRoleIds.ToList()
        }));

        Assert.Contains("Username already exists.", exception.Errors);
        Assert.Contains("Email already exists.", exception.Errors);
        Assert.Contains("One or more roles do not exist.", exception.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProfileAndPassword_WhenRequestIsValid()
    {
        var role = CreateRole("Administrator");
        var user = CreateLoadedUser(Guid.NewGuid(), "alice", "Alice", "alice@example.com", true, role);
        var previousHash = user.PasswordHash;

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepository.Setup(x => x.ExistsByUserNameAsync("alice", user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        userRepository.Setup(x => x.ExistsByEmailAsync("alice.updated@example.com", user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { role });

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            userRepository.Object,
            roleRepository.Object,
            unitOfWork.Object,
            NullLogger<UserService>.Instance);

        var result = await service.UpdateAsync(user.Id, new UpdateUserDto
        {
            FullName = "  Alice Updated  ",
            Email = "  alice.updated@example.com  ",
            Password = "NewPass123!"
        });

        Assert.Equal("Alice Updated", user.FullName);
        Assert.Equal("alice.updated@example.com", user.Email);
        Assert.NotEqual(previousHash, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass123!", user.PasswordHash));
        Assert.Equal("Alice Updated", result.FullName);
        Assert.Equal("alice.updated@example.com", result.Email);

        userRepository.Verify(x => x.Update(user), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldPersistNewStatus()
    {
        var user = CreateLoadedUser(Guid.NewGuid(), "alice", "Alice", "alice@example.com", true, CreateRole("Administrator"));

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            userRepository.Object,
            Mock.Of<IRoleRepository>(),
            unitOfWork.Object,
            NullLogger<UserService>.Instance);

        var result = await service.UpdateStatusAsync(user.Id, false);

        Assert.False(user.IsActive);
        Assert.False(result.IsActive);
        userRepository.Verify(x => x.Update(user), Times.Once);
    }

    [Fact]
    public async Task AssignRolesAsync_ShouldReplaceRolesAndReturnUpdatedUser()
    {
        var user = CreateLoadedUser(Guid.NewGuid(), "alice", "Alice", "alice@example.com", true, CreateRole("OldRole"));
        var newRoles = new[]
        {
            CreateRole("Administrator"),
            CreateRole("Auditor")
        };
        var updatedUser = CreateLoadedUser(user.Id, user.UserName, user.FullName, user.Email, user.IsActive, newRoles);

        var userRepository = new Mock<IUserRepository>();
        userRepository.SetupSequence(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user)
            .ReturnsAsync(updatedUser);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRoles);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            userRepository.Object,
            roleRepository.Object,
            unitOfWork.Object,
            NullLogger<UserService>.Instance);

        var result = await service.AssignRolesAsync(user.Id, new AssignUserRolesDto
        {
            RoleIds = newRoles.Select(x => x.Id).ToList()
        });

        Assert.Equal(2, user.UserRoles.Count);
        Assert.DoesNotContain(user.UserRoles, x => x.RoleId == Guid.Empty);
        Assert.Equal(2, result.Roles.Count);
        Assert.Contains(result.Roles, x => x.Name == "Administrator");
        Assert.Contains(result.Roles, x => x.Name == "Auditor");

        userRepository.Verify(x => x.Update(user), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRolesAsync_ShouldThrowValidationException_WhenAnyRoleIsMissing()
    {
        var user = CreateLoadedUser(Guid.NewGuid(), "alice", "Alice", "alice@example.com", true, CreateRole("OldRole"));
        var requestedRoleIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreateRole("Administrator", requestedRoleIds[0]) });

        var service = new UserService(
            userRepository.Object,
            roleRepository.Object,
            Mock.Of<IUnitOfWork>(),
            NullLogger<UserService>.Instance);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.AssignRolesAsync(user.Id, new AssignUserRolesDto
        {
            RoleIds = requestedRoleIds.ToList()
        }));

        Assert.Contains("One or more roles do not exist.", exception.Errors);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveExistingUser()
    {
        var user = CreateLoadedUser(Guid.NewGuid(), "alice", "Alice", "alice@example.com", true, CreateRole("Administrator"));

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new UserService(
            userRepository.Object,
            Mock.Of<IRoleRepository>(),
            unitOfWork.Object,
            NullLogger<UserService>.Instance);

        await service.DeleteAsync(user.Id);

        userRepository.Verify(x => x.Remove(user), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Role CreateRole(string name, Guid? id = null)
    {
        return new Role
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = $"{name} role"
        };
    }

    private static User CreateUser(string userName, string fullName, string email, Role role)
    {
        return CreateLoadedUser(Guid.NewGuid(), userName, fullName, email, true, role);
    }

    private static User CreateLoadedUser(Guid id, string userName, string fullName, string email, bool isActive, params Role[] roles)
    {
        var user = new User
        {
            Id = id,
            UserName = userName,
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            IsActive = isActive
        };

        user.UserRoles = roles.Select(role => new UserRole
        {
            User = user,
            UserId = user.Id,
            Role = role,
            RoleId = role.Id
        }).ToList();

        return user;
    }
}
