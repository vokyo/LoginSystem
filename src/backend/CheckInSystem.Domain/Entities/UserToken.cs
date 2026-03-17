namespace CheckInSystem.Domain.Entities;

public sealed class UserToken : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string TokenId { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public bool IsCurrent { get; set; } = true;
    public string? GeneratedBy { get; set; }

    public User User { get; set; } = null!;
}
