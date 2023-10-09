using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace ApplicationCore.Data
{
    public class AppRoleClaim : IdentityRoleClaim<string>, IAuditableEntity<int>
    {
        public virtual DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public virtual DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public virtual string CreatedBy { get; set; } = "?";
        public virtual string UpdatedBy { get; set; } = "?";
        public virtual string ExternalId { get; set; } = "";

        public string Description { get; set; } = "";
        public string Group { get; set; } = "";

        public virtual AppRole Role { get; set; }
    }
}
