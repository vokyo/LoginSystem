using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Repositories;

public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.UserName == userName, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetPagedAsync(string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await BuildQuery(search, isActive)
            .OrderBy(x => x.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? search, bool? isActive, CancellationToken cancellationToken = default)
    {
        return BuildQuery(search, isActive).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.Users.Where(x => idList.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(x => x.UserName == userName && (!excludeId.HasValue || x.Id != excludeId), cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(x => x.Email == email && (!excludeId.HasValue || x.Id != excludeId), cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return context.Users.AddAsync(user, cancellationToken).AsTask();
    }

    public void Update(User user)
    {
        context.Users.Update(user);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }

    private IQueryable<User> BuildQuery(string? search, bool? isActive)
    {
        var query = context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.UserName.Contains(search) || x.FullName.Contains(search) || x.Email.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return query;
    }
}
