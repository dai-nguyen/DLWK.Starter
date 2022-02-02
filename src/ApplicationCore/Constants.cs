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

        public static class Claims
        {
            // users
            public static string can_read_user = "can_read_user";
            public static string can_edit_user = "can_edit_user";
            public static string can_create_user = "can_create_user";
            public static string can_delete_user = "can_delete_user";

            // roles
            public static string can_read_role = "can_read_role";
            public static string can_edit_role = "can_edit_role";
            public static string can_create_role = "can_create_role";
            public static string can_delete_role = "can_delete_role";
        }

        public static IEnumerable<RolePermission> PermissionCheckList => new RolePermission[]
        {
            new RolePermission(ClaimNames.role, "Roles"),
            new RolePermission(ClaimNames.user, "Users"),
        };

    }
}
