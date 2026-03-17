using System.ComponentModel.DataAnnotations;

namespace CheckInSystem.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
