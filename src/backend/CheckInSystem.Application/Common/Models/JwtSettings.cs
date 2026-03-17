namespace CheckInSystem.Application.Common.Models;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int AdminTokenExpiryMinutes { get; set; } = 120;
    public int CheckInTokenExpiryHours { get; set; } = 24;
}
