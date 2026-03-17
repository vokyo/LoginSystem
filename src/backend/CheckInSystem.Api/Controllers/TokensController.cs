using System.Security.Claims;
using CheckInSystem.Api.Extensions;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Tokens;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TokensController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("users/{userId:guid}/generate")]
    [Authorize(Policy = PermissionPolicies.TokensWrite)]
    public async Task<IActionResult> Generate(Guid userId, [FromBody] GenerateUserTokenRequestDto request, CancellationToken cancellationToken)
    {
        var generatedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "system";
        var response = await tokenService.GenerateAsync(userId, request, generatedBy, cancellationToken);
        return Ok(ApiResponse.Success(response, "Token generated successfully.", 201));
    }

    [HttpGet("users/{userId:guid}/current")]
    [Authorize(Policy = PermissionPolicies.TokensRead)]
    public async Task<IActionResult> GetCurrent(Guid userId, CancellationToken cancellationToken)
    {
        var response = await tokenService.GetCurrentAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(response, "Current token fetched successfully."));
    }

    [HttpGet("users/{userId:guid}")]
    [Authorize(Policy = PermissionPolicies.TokensRead)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        var response = await tokenService.GetByUserAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(response, "Token history fetched successfully."));
    }

    [HttpPost("users/{userId:guid}/revoke-current")]
    [Authorize(Policy = PermissionPolicies.TokensWrite)]
    public async Task<IActionResult> RevokeCurrent(Guid userId, CancellationToken cancellationToken)
    {
        var revokedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "system";
        var response = await tokenService.RevokeCurrentAsync(userId, revokedBy, cancellationToken);
        return Ok(ApiResponse.Success(response, "Current token revoked successfully."));
    }
}
