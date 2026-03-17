namespace CheckInSystem.Domain.Entities;

public sealed class User : BaseAuditableEntity
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
    public ICollection<CheckInRecord> CheckInRecords { get; set; } = new List<CheckInRecord>();
}
