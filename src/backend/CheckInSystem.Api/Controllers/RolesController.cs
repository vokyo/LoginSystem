using CheckInSystem.Api.Extensions;
using CheckInSystem.Application.Common.Models;
using CheckInSystem.Application.DTOs.Roles;
using CheckInSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheckInSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionPolicies.RolesRead)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await roleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse.Success(response, "Roles fetched successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesRead)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await roleService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse.Success(response, "Role fetched successfully."));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionPolicies.RolesRead)]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
    {
        var response = await roleService.GetPermissionsAsync(cancellationToken);
        return Ok(ApiResponse.Success(response, "Permissions fetched successfully."));
    }

    [HttpPost]
    [Authorize(Policy = PermissionPolicies.RolesWrite)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto request, CancellationToken cancellationToken)
    {
        var response = await roleService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse.Success(response, "Role created successfully.", 201));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto request, CancellationToken cancellationToken)
    {
        var response = await roleService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse.Success(response, "Role updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await roleService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse.Success<object?>(null, "Role deleted successfully."));
    }
}
