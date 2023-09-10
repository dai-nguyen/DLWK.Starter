using ApplicationCore.Models;

namespace ApplicationCore.Constants
{
    public static class Const
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
            public const string bulk = "bulk";
        }

        public static IEnumerable<RolePermission> PermissionCheckList => new RolePermission[]
        {
            new RolePermission(ClaimNames.roles, "Roles"),
            new RolePermission(ClaimNames.users, "Users"),
        };

        public static class LocalStorageKeys
        {
            public const string ProfilePicture = "ProfilePicture";
            public const string ProfileFullName = "ProfileFullName";
            public const string ProfileTitle = "ProfileTitle";
        }

        public static class Messages
        {
            public const string PermissionDenied = "Permission denied";
            public const string InternalError = "InternalError";
            public const string NotFound = "NotFound";
            public const string Saved = "Saved";
            public const string Deleted = "Deleted";
        }

        public static class BulkJobStatus
        {
            public const string Pending = "Pending";
            public const string Completed = "Completed";

        }
    }
}
