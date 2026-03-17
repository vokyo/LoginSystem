using CheckInSystem.Api.Extensions;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.CheckIns;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CheckInRecordsController(ICheckInService checkInService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.CheckInsRead)]
    public async Task<IActionResult> GetPaged([FromQuery] CheckInRecordQueryDto query, CancellationToken cancellationToken)
    {
        var response = await checkInService.GetRecordsAsync(query, cancellationToken);
        return Ok(ApiResponse.Success(response, "Check-in records fetched successfully."));
    }
}
