using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.DTOs.Auth;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CheckInSystem.Infrastructure.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUserNameAsync(request.UserName, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("The user is disabled.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = jwtTokenService.GenerateAdminToken(user);
        logger.LogInformation("Admin user {UserName} logged in successfully.", user.UserName);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            UserId = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            Roles = user.UserRoles.Select(x => x.Role.Name).ToArray(),
            Permissions = user.UserRoles
                .SelectMany(x => x.Role.RolePermissions.Select(rp => rp.Permission.Code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };
    }
}
