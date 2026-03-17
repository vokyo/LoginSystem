using System.ComponentModel.DataAnnotations;

namespace CheckInSystem.Application.DTOs.Tokens;

public sealed class GenerateUserTokenRequestDto
{
    [Range(1, 720)]
    public int ValidHours { get; set; } = 24;
}

public sealed class UserTokenDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public bool IsCurrent { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RevokedReason { get; set; }
}

public sealed class TokenGenerationResultDto
{
    public string Token { get; set; } = string.Empty;
    public UserTokenDto TokenInfo { get; set; } = new();
}
