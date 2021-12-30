using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Identity;

namespace ApplicationCore.Data
{
    public class AppUser : IdentityUser<string>, IAuditableCustomAttributeEntity<string>
    {
        public virtual DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public virtual DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public virtual string CreatedBy { get; set; } = "?";
        public virtual string UpdatedBy { get; set; } = "?";
        public virtual string ExternalId { get; set; } = "";

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string ProfilePictureUrl { get; set; } = "";

        public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
    }
}
