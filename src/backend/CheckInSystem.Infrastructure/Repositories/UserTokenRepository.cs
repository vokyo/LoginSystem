using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Repositories;

public sealed class UserTokenRepository(ApplicationDbContext context) : IUserTokenRepository
{
    public async Task<UserToken?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserTokens
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsCurrent, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserTokens
            .Include(x => x.User)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserToken?> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        return await context.UserTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenId == tokenId, cancellationToken);
    }

    public Task AddAsync(UserToken token, CancellationToken cancellationToken = default)
    {
        return context.UserTokens.AddAsync(token, cancellationToken).AsTask();
    }

    public void Update(UserToken token)
    {
        context.UserTokens.Update(token);
    }
}
