using CheckInSystem.Api.Extensions;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Users;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.UsersRead)]
    public async Task<IActionResult> GetPaged([FromQuery] UserListQueryDto query, CancellationToken cancellationToken)
    {
        var response = await userService.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse.Success(response, "Users fetched successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await userService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse.Success(response, "User fetched successfully."));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto request, CancellationToken cancellationToken)
    {
        var response = await userService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse.Success(response, "User created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken)
    {
        var response = await userService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse.Success(response, "User updated successfully."));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusDto request, CancellationToken cancellationToken)
    {
        var response = await userService.UpdateStatusAsync(id, request.IsActive, cancellationToken);
        return Ok(ApiResponse.Success(response, "User status updated successfully."));
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignUserRolesDto request, CancellationToken cancellationToken)
    {
        var response = await userService.AssignRolesAsync(id, request, cancellationToken);
        return Ok(ApiResponse.Success(response, "User roles updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.UsersWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Success<object?>(null, "User deleted successfully."));
    }
}
