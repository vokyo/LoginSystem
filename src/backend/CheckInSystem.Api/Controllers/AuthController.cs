using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Auth;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse.Success(response, "Login successful."));
    }
}
