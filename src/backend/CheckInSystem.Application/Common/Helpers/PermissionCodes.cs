namespace CheckInSystem.Application.Common.Helpers;

public static class PermissionCodes
{
    public const string UsersRead = "users.read";
    public const string UsersWrite = "users.write";
    public const string RolesRead = "roles.read";
    public const string RolesWrite = "roles.write";
    public const string TokensRead = "tokens.read";
    public const string TokensWrite = "tokens.write";
    public const string CheckInsRead = "checkins.read";

    public static readonly IReadOnlyCollection<(string Name, string Code, string Description)> All =
    [
        ("Read Users", UsersRead, "View user information."),
        ("Manage Users", UsersWrite, "Create, update, delete, enable and disable users."),
        ("Read Roles", RolesRead, "View role and permission assignments."),
        ("Manage Roles", RolesWrite, "Create, update and delete roles."),
        ("Read Tokens", TokensRead, "View token status and token history."),
        ("Manage Tokens", TokensWrite, "Generate and revoke user tokens."),
        ("Read Check-In Records", CheckInsRead, "View check-in records and filters.")
    ];
}
