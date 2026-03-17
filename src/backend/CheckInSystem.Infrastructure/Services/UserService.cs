using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Users;
using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Application.Interfaces.Services;
using CheckInSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CheckInSystem.Infrastructure.Services;

public sealed class UserService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserService> logger) : IUserService
{
    public async Task<PagedResult<UserDto>> GetPagedAsync(UserListQueryDto query, CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetPagedAsync(query.Search, query.IsActive, query.PageNumber, query.PageSize, cancellationToken);
        var totalCount = await userRepository.CountAsync(query.Search, query.IsActive, cancellationToken);

        return new PagedResult<UserDto>
        {
            Items = users.Select(x => x.ToDto()).ToArray(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto request, CancellationToken cancellationToken = default)
    {
        await ValidateUserAsync(request.UserName, request.Email, request.RoleIds, null, cancellationToken);

        var user = new User
        {
            UserName = request.UserName.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = request.IsActive
        };

        var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);
        user.UserRoles = roles.Select(x => new UserRole { User = user, RoleId = x.Id }).ToList();

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserName} created.", user.UserName);
        var createdUser = await userRepository.GetByIdAsync(user.Id, cancellationToken) ?? throw new KeyNotFoundException("User not found after creation.");
        return createdUser.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");

        await ValidateUserAsync(user.UserName, request.Email, user.UserRoles.Select(x => x.RoleId), id, cancellationToken);
        user.FullName = request.FullName.Trim();
        user.Email = request.Email.Trim();

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = isActive;
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserDto> AssignRolesAsync(Guid id, AssignUserRolesDto request, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        var roles = await roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);

        if (roles.Count != request.RoleIds.Count)
        {
            throw new ValidationException(["One or more roles do not exist."]);
        }

        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedUser = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        return updatedUser.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        userRepository.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateUserAsync(string userName, string email, IEnumerable<Guid> roleIds, Guid? excludeId, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (await userRepository.ExistsByUserNameAsync(userName.Trim(), excludeId, cancellationToken))
        {
            errors.Add("Username already exists.");
        }

        if (await userRepository.ExistsByEmailAsync(email.Trim(), excludeId, cancellationToken))
        {
            errors.Add("Email already exists.");
        }

        var roles = await roleRepository.GetByIdsAsync(roleIds, cancellationToken);
        if (roles.Count != roleIds.Distinct().Count())
        {
            errors.Add("One or more roles do not exist.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
