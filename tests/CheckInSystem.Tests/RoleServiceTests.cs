using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.DTOs.Roles;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Services;
using Moq;

namespace CheckInSystem.Tests;

public sealed class RoleServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedRoles()
    {
        var roles = new[]
        {
            CreateRole("Administrator", false, [CreatePermission("Users Read", "users.read")]),
            CreateRole("Auditor", false, [CreatePermission("Records Read", "records.read")])
        };

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var service = new RoleService(
            roleRepository.Object,
            Mock.Of<IPermissionRepository>(),
            Mock.Of<IUnitOfWork>());

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Name == "Administrator");
        Assert.Contains(result, x => x.Name == "Auditor");
    }

    [Fact]
    public async Task CreateAsync_ShouldTrimFieldsAssignPermissionsAndReturnCreatedRole()
    {
        var permissions = new[]
        {
            CreatePermission("Users Read", "users.read"),
            CreatePermission("Users Write", "users.write")
        };

        Role? addedRole = null;

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.ExistsByNameAsync("Administrators", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        roleRepository.Setup(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Callback<Role, CancellationToken>((role, _) => addedRole = role)
            .Returns(Task.CompletedTask);
        roleRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                if (addedRole is null || addedRole.Id != id)
                {
                    return null;
                }

                return CreateRole(
                    addedRole.Name,
                    addedRole.IsSystem,
                    permissions,
                    addedRole.Id,
                    addedRole.Description,
                    addedRole.IsActive);
            });

        var permissionRepository = new Mock<IPermissionRepository>();
        permissionRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new RoleService(
            roleRepository.Object,
            permissionRepository.Object,
            unitOfWork.Object);

        var result = await service.CreateAsync(new CreateRoleDto
        {
            Name = "  Administrators  ",
            Description = "  Administrators group  ",
            IsActive = true,
            PermissionIds = permissions.Select(x => x.Id).ToList()
        });

        Assert.NotNull(addedRole);
        Assert.Equal("Administrators", addedRole!.Name);
        Assert.Equal("Administrators group", addedRole.Description);
        Assert.Equal(2, addedRole.RolePermissions.Count);
        Assert.Equal(2, result.Permissions.Count);
        Assert.Contains(result.Permissions, x => x.Code == "users.read");
        Assert.Contains(result.Permissions, x => x.Code == "users.write");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowValidationException_WhenRoleNameExistsOrPermissionsMissing()
    {
        var requestedPermissionIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.ExistsByNameAsync("Administrator", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var permissionRepository = new Mock<IPermissionRepository>();
        permissionRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { CreatePermission("Users Read", "users.read", requestedPermissionIds[0]) });

        var service = new RoleService(
            roleRepository.Object,
            permissionRepository.Object,
            Mock.Of<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(new CreateRoleDto
        {
            Name = "Administrator",
            Description = "Admins",
            PermissionIds = requestedPermissionIds.ToList()
        }));

        Assert.Contains("Role name already exists.", exception.Errors);
        Assert.Contains("One or more permissions do not exist.", exception.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplacePermissionsAndReturnUpdatedRole()
    {
        var existingRole = CreateRole("Administrator", false, [CreatePermission("Users Read", "users.read")]);
        var updatedPermissions = new[]
        {
            CreatePermission("Users Read", "users.read"),
            CreatePermission("Users Write", "users.write")
        };
        var updatedRole = CreateRole(existingRole.Name, false, updatedPermissions, existingRole.Id, existingRole.Description, existingRole.IsActive);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.SetupSequence(x => x.GetByIdAsync(existingRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole)
            .ReturnsAsync(updatedRole);
        roleRepository.Setup(x => x.ExistsByNameAsync("Platform Admin", existingRole.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var permissionRepository = new Mock<IPermissionRepository>();
        permissionRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPermissions);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new RoleService(
            roleRepository.Object,
            permissionRepository.Object,
            unitOfWork.Object);

        var result = await service.UpdateAsync(existingRole.Id, new UpdateRoleDto
        {
            Name = "  Platform Admin  ",
            Description = "  Elevated access  ",
            IsActive = true,
            PermissionIds = updatedPermissions.Select(x => x.Id).ToList()
        });

        Assert.Equal("Platform Admin", existingRole.Name);
        Assert.Equal("Elevated access", existingRole.Description);
        Assert.Equal(2, existingRole.RolePermissions.Count);
        Assert.All(existingRole.RolePermissions, permission => Assert.Equal(existingRole.Id, permission.RoleId));
        Assert.Equal(2, result.Permissions.Count);
        Assert.Contains(result.Permissions, x => x.Code == "users.write");

        roleRepository.Verify(x => x.Update(existingRole), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowBusinessException_WhenRoleIsSystemRole()
    {
        var role = CreateRole("Administrator", true, [CreatePermission("Users Read", "users.read")]);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var service = new RoleService(
            roleRepository.Object,
            Mock.Of<IPermissionRepository>(),
            Mock.Of<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.DeleteAsync(role.Id));

        Assert.Equal("System roles cannot be deleted.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRole_WhenRoleIsNotSystem()
    {
        var role = CreateRole("Auditor", false, [CreatePermission("Records Read", "records.read")]);

        var roleRepository = new Mock<IRoleRepository>();
        roleRepository.Setup(x => x.GetByIdAsync(role.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new RoleService(
            roleRepository.Object,
            Mock.Of<IPermissionRepository>(),
            unitOfWork.Object);

        await service.DeleteAsync(role.Id);

        roleRepository.Verify(x => x.Remove(role), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPermissionsAsync_ShouldReturnMappedPermissionDtos()
    {
        var permissions = new[]
        {
            CreatePermission("Users Read", "users.read"),
            CreatePermission("Users Write", "users.write")
        };

        var permissionRepository = new Mock<IPermissionRepository>();
        permissionRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var service = new RoleService(
            Mock.Of<IRoleRepository>(),
            permissionRepository.Object,
            Mock.Of<IUnitOfWork>());

        var result = await service.GetPermissionsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Code == "users.read");
        Assert.Contains(result, x => x.Code == "users.write");
    }

    private static Role CreateRole(
        string name,
        bool isSystem,
        IEnumerable<Permission> permissions,
        Guid? id = null,
        string? description = null,
        bool isActive = true)
    {
        var role = new Role
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = description ?? $"{name} description",
            IsActive = isActive,
            IsSystem = isSystem
        };

        role.RolePermissions = permissions.Select(permission => new RolePermission
        {
            Role = role,
            RoleId = role.Id,
            Permission = permission,
            PermissionId = permission.Id
        }).ToList();

        return role;
    }

    private static Permission CreatePermission(string name, string code, Guid? id = null)
    {
        return new Permission
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = $"{name} description"
        };
    }
}
