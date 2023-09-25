using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using NodaTime;
using NpgsqlTypes;

namespace ApplicationCore.Data
{
    public class AppRole : IdentityRole, IAuditableEntity<string>
    {
        public virtual Instant DateCreated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public virtual Instant DateUpdated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public virtual string CreatedBy { get; set; } = "?";
        public virtual string UpdatedBy { get; set; } = "?";
        public virtual string ExternalId { get; set; } = "";

        public string Description { get; set; } = "";

        public NpgsqlTsVector SearchVector { get; set; }

        //public virtual IEnumerable<AppRoleClaim> RoleClaims { get; set; } = Enumerable.Empty<AppRoleClaim>();
    }
}
