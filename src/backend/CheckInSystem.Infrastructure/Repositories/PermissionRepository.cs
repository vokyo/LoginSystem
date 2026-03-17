using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Repositories;

public sealed class PermissionRepository(ApplicationDbContext context) : IPermissionRepository
{
    public async Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Permissions.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.Permissions.Where(x => idList.Contains(x.Id)).ToListAsync(cancellationToken);
    }
}
