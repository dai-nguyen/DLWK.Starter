using ApplicationCore.Models;

namespace ApplicationCore
{
    public static class Constants
    {
        public static class ClaimNames
        {
            public const string users = "users";
            public const string roles = "roles";
        }

        public static class Permissions
        {
            public const string read = "read";
            public const string edit = "edit";
            public const string create = "create";
            public const string delete = "delete";
        }

        public static IEnumerable<RolePermission> PermissionCheckList => new RolePermission[]
        {
            new RolePermission(ClaimNames.roles, "Roles"),
            new RolePermission(ClaimNames.users, "Users"),
        };

    }
}
