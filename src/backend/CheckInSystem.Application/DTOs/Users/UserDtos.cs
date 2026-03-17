using System.ComponentModel.DataAnnotations;

namespace CheckInSystem.Application.DTOs.Users;

public sealed class CreateUserDto
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class UpdateUserDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Password { get; set; }
}

public sealed class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}

public sealed class AssignUserRolesDto
{
    [Required]
    public List<Guid> RoleIds { get; set; } = [];
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyCollection<RoleLookupDto> Roles { get; set; } = Array.Empty<RoleLookupDto>();
}

public sealed class UserListQueryDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class RoleLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
