using CheckInSystem.Application.DTOs.CheckIns;
using CheckInSystem.Application.DTOs.Roles;
using CheckInSystem.Application.DTOs.Tokens;
using CheckInSystem.Application.DTOs.Users;
using CheckInSystem.Domain.Entities;

namespace CheckInSystem.Infrastructure.Services;

internal static class MappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc,
            Roles = user.UserRoles
                .Select(x => new RoleLookupDto
                {
                    Id = x.RoleId,
                    Name = x.Role.Name
                })
                .ToArray()
        };
    }

    public static RoleDto ToDto(this Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystem = role.IsSystem,
            Permissions = role.RolePermissions
                .Select(x => new PermissionDto
                {
                    Id = x.PermissionId,
                    Name = x.Permission.Name,
                    Code = x.Permission.Code,
                    Description = x.Permission.Description
                })
                .OrderBy(x => x.Name)
                .ToArray()
        };
    }

    public static UserTokenDto ToDto(this UserToken token)
    {
        return new UserTokenDto
        {
            Id = token.Id,
            UserId = token.UserId,
            UserName = token.User.UserName,
            TokenId = token.TokenId,
            IssuedAtUtc = token.IssuedAtUtc,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RevokedAtUtc = token.RevokedAtUtc,
            IsCurrent = token.IsCurrent,
            RevokedReason = token.RevokedReason,
            Status = token.RevokedAtUtc.HasValue
                ? "Revoked"
                : token.ExpiresAtUtc <= DateTime.UtcNow
                    ? "Expired"
                    : token.IsCurrent
                        ? "Valid"
                        : "Superseded"
        };
    }

    public static CheckInRecordDto ToDto(this CheckInRecord record)
    {
        return new CheckInRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = record.User?.UserName,
            Status = record.Status.ToString(),
            FailureReason = record.FailureReason,
            CheckedInAtUtc = record.CheckedInAtUtc,
            SourceIp = record.SourceIp,
            UserAgent = record.UserAgent
        };
    }
}
