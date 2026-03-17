using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.CheckIns;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public sealed class CheckInController(ICheckInService checkInService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request, CancellationToken cancellationToken)
    {
        var token = request.Token;
        if (string.IsNullOrWhiteSpace(token) &&
            HttpContext.Request.Headers.Authorization.Count > 0)
        {
            var bearer = HttpContext.Request.Headers.Authorization.ToString();
            token = bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? bearer["Bearer ".Length..].Trim()
                : bearer.Trim();
        }

        var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var response = await checkInService.CheckInAsync(token ?? string.Empty, sourceIp, userAgent, cancellationToken);

        if (response.IsSuccessful)
        {
            return Ok(ApiResponse.Success(response, response.Message));
        }

        return BadRequest(ApiResponse.Fail(response.FailureReason ?? response.Message, 400, response));
    }
}
