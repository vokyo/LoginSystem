using CheckInSystem.Application.Common.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace CheckInSystem.Api.Extensions;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Claims.Any(x => x.Type == "permission" && string.Equals(x.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class PermissionPolicies
{
    public const string UsersRead = nameof(UsersRead);
    public const string UsersWrite = nameof(UsersWrite);
    public const string RolesRead = nameof(RolesRead);
    public const string RolesWrite = nameof(RolesWrite);
    public const string TokensRead = nameof(TokensRead);
    public const string TokensWrite = nameof(TokensWrite);
    public const string CheckInsRead = nameof(CheckInsRead);

    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(UsersRead, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersRead)));
        options.AddPolicy(UsersWrite, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.UsersWrite)));
        options.AddPolicy(RolesRead, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.RolesRead)));
        options.AddPolicy(RolesWrite, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.RolesWrite)));
        options.AddPolicy(TokensRead, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.TokensRead)));
        options.AddPolicy(TokensWrite, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.TokensWrite)));
        options.AddPolicy(CheckInsRead, policy => policy.Requirements.Add(new PermissionRequirement(PermissionCodes.CheckInsRead)));
    }
}
