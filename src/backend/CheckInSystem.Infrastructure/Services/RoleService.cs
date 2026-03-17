using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.DTOs.Roles;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;

namespace CheckInSystem.Infrastructure.Services;

public sealed class RoleService(
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<IReadOnlyCollection<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(x => x.ToDto()).ToArray();
    }

    public async Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Role not found.");
        return role.ToDto();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken = default)
    {
        await ValidateRoleAsync(request.Name, request.PermissionIds, null, cancellationToken);

        var permissions = await permissionRepository.GetByIdsAsync(request.PermissionIds, cancellationToken);
        var role = new Role
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IsActive = request.IsActive,
            RolePermissions = permissions.Select(x => new RolePermission
            {
                PermissionId = x.Id
            }).ToList()
        };

        await roleRepository.AddAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdRole = await roleRepository.GetByIdAsync(role.Id, cancellationToken) ?? throw new KeyNotFoundException("Role not found.");
        return createdRole.ToDto();
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto request, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Role not found.");
        await ValidateRoleAsync(request.Name, request.PermissionIds, id, cancellationToken);

        var permissions = await permissionRepository.GetByIdsAsync(request.PermissionIds, cancellationToken);
        role.Name = request.Name.Trim();
        role.Description = request.Description.Trim();
        role.IsActive = request.IsActive;
        role.RolePermissions.Clear();

        foreach (var permission in permissions)
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        roleRepository.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRole = await roleRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Role not found.");
        return updatedRole.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Role not found.");

        if (role.IsSystem)
        {
            throw new BusinessException("System roles cannot be deleted.");
        }

        roleRepository.Remove(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await permissionRepository.GetAllAsync(cancellationToken);
        return permissions.Select(x => new PermissionDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Description = x.Description
        }).ToArray();
    }

    private async Task ValidateRoleAsync(string name, IEnumerable<Guid> permissionIds, Guid? excludeId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (await roleRepository.ExistsByNameAsync(name.Trim(), excludeId, cancellationToken))
        {
            errors.Add("Role name already exists.");
        }

        var permissionList = permissionIds.Distinct().ToList();
        var permissions = await permissionRepository.GetByIdsAsync(permissionList, cancellationToken);
        if (permissions.Count != permissionList.Count)
        {
            errors.Add("One or more permissions do not exist.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
