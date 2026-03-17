using System.ComponentModel.DataAnnotations;

namespace CheckInSystem.Application.DTOs.Roles;

public sealed class CreateRoleDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public List<Guid> PermissionIds { get; set; } = [];
}

public sealed class UpdateRoleDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public List<Guid> PermissionIds { get; set; } = [];
}

public sealed class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public IReadOnlyCollection<PermissionDto> Permissions { get; set; } = Array.Empty<PermissionDto>();
}

public sealed class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
