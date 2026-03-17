using CheckInSystem.Domain.Enums;

namespace CheckInSystem.Domain.Entities;

public sealed class CheckInRecord : BaseAuditableEntity
{
    public Guid? UserId { get; set; }
    public Guid? UserTokenId { get; set; }
    public string? SubmittedTokenId { get; set; }
    public CheckInRecordStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CheckedInAtUtc { get; set; }
    public string? SourceIp { get; set; }
    public string? UserAgent { get; set; }

    public User? User { get; set; }
    public UserToken? UserToken { get; set; }
}
