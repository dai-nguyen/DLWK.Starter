using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;
using NodaTime;
using NpgsqlTypes;

namespace ApplicationCore.Data
{
    public class AppUser : IdentityUser<string>, IAuditableCustomAttributeEntity<string>
    {
        public virtual Instant DateCreated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public virtual Instant DateUpdated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public virtual string CreatedBy { get; set; } = "?";
        public virtual string UpdatedBy { get; set; } = "?";
        public virtual string ExternalId { get; set; } = "";

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Title { get; set; } = "";
        public string SecurityCode { get; set; }
        public string ProfilePicture { get; set; } = "";

        public NpgsqlTsVector SearchVector { get; set; }

        public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
        
    }
}
