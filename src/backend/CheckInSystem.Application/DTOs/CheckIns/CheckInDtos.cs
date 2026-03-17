namespace CheckInSystem.Application.DTOs.CheckIns;

public sealed class CheckInRequestDto
{
    public string? Token { get; set; }
}

public sealed class CheckInResultDto
{
    public bool IsSuccessful { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CheckedInAtUtc { get; set; }
    public string? FailureReason { get; set; }
}

public sealed class CheckInRecordQueryDto
{
    public Guid? UserId { get; set; }
    public DateTime? StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class CheckInRecordDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public DateTime CheckedInAtUtc { get; set; }
    public string? SourceIp { get; set; }
    public string? UserAgent { get; set; }
}
