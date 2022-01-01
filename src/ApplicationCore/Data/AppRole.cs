using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ApplicationCore.Data
{
    public class AppRole : IdentityRole, IAuditableEntity<string>
    {
        public virtual DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public virtual DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public virtual string CreatedBy { get; set; } = "?";
        public virtual string UpdatedBy { get; set; } = "?";
        public virtual string ExternalId { get; set; } = "";

        public string Description { get; set; } = "";

        public virtual IEnumerable<AppRoleClaim> RoleClaims { get; set; } = Enumerable.Empty<AppRoleClaim>();
    }
}
