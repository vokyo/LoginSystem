using CheckInSystem.Domain.Entities;

namespace CheckInSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetPagedAsync(string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, bool? isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Remove(User user);
}

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    void Update(Role role);
    void Remove(Role role);
}

public interface IPermissionRepository
{
    Task<IReadOnlyCollection<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}

public interface IUserTokenRepository
{
    Task<UserToken?> GetCurrentByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserToken?> GetByTokenIdAsync(string tokenId, CancellationToken cancellationToken = default);
    Task AddAsync(UserToken token, CancellationToken cancellationToken = default);
    void Update(UserToken token);
}

public interface ICheckInRecordRepository
{
    Task AddAsync(CheckInRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CheckInRecord>> GetPagedAsync(Guid? userId, DateTime? startUtc, DateTime? endUtc, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? userId, DateTime? startUtc, DateTime? endUtc, string? status, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
