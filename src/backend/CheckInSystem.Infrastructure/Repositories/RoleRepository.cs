using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Repositories;

public sealed class RoleRepository(ApplicationDbContext context) : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Roles
            .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.Roles.Where(x => idList.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return context.Roles.AnyAsync(x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId), cancellationToken);
    }

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        return context.Roles.AddAsync(role, cancellationToken).AsTask();
    }

    public void Update(Role role)
    {
        context.Roles.Update(role);
    }

    public void Remove(Role role)
    {
        context.Roles.Remove(role);
    }
}
