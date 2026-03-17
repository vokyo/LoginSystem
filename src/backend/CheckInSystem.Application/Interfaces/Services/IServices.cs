using System.Security.Claims;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Auth;
using CheckInSystem.Application.DTOs.CheckIns;
using CheckInSystem.Application.DTOs.Roles;
using CheckInSystem.Application.DTOs.Tokens;
using CheckInSystem.Application.DTOs.Users;
using CheckInSystem.Domain.Entities;

namespace CheckInSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserListQueryDto query, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
    Task<UserDto> AssignRolesAsync(Guid id, AssignUserRolesDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(CreateRoleDto request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    Task<TokenGenerationResultDto> GenerateAsync(Guid userId, GenerateUserTokenRequestDto request, string generatedBy, CancellationToken cancellationToken = default);
    Task<UserTokenDto?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserTokenDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface ICheckInService
{
    Task<CheckInResultDto> CheckInAsync(string token, string? sourceIp, string? userAgent, CancellationToken cancellationToken = default);
    Task<PagedResult<CheckInRecordDto>> GetRecordsAsync(CheckInRecordQueryDto query, CancellationToken cancellationToken = default);
}

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAdminToken(User user);
    (string Token, string TokenId, DateTime IssuedAtUtc, DateTime ExpiresAtUtc) GenerateCheckInToken(User user, DateTime expiresAtUtc);
    ClaimsPrincipal ValidateCheckInToken(string token);
    (Guid? UserId, string? TokenId) ReadIdentifiers(string token);
}
