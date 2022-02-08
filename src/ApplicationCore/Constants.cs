using ApplicationCore.Models;

namespace ApplicationCore
{
    public static class Constants
    {
        public static class ClaimNames
        {
            public const string user = "user";
            public const string role = "roles";
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
            new RolePermission(ClaimNames.role, "Roles"),
            new RolePermission(ClaimNames.user, "Users"),
        };

    }
}
