using CheckInSystem.Application.Interfaces.Repositories;
using CheckInSystem.Domain.Entities;
using CheckInSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Repositories;

public sealed class CheckInRecordRepository(ApplicationDbContext context) : ICheckInRecordRepository
{
    public Task AddAsync(CheckInRecord record, CancellationToken cancellationToken = default)
    {
        return context.CheckInRecords.AddAsync(record, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyCollection<CheckInRecord>> GetPagedAsync(Guid? userId, DateTime? startUtc, DateTime? endUtc, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await BuildQuery(userId, startUtc, endUtc, status)
            .Include(x => x.User)
            .OrderByDescending(x => x.CheckedInAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid? userId, DateTime? startUtc, DateTime? endUtc, string? status, CancellationToken cancellationToken = default)
    {
        return BuildQuery(userId, startUtc, endUtc, status).CountAsync(cancellationToken);
    }

    private IQueryable<CheckInRecord> BuildQuery(Guid? userId, DateTime? startUtc, DateTime? endUtc, string? status)
    {
        var query = context.CheckInRecords.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        if (startUtc.HasValue)
        {
            query = query.Where(x => x.CheckedInAtUtc >= startUtc.Value);
        }

        if (endUtc.HasValue)
        {
            query = query.Where(x => x.CheckedInAtUtc <= endUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.CheckInRecordStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        return query;
    }
}
