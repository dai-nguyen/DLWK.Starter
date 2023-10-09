using ApplicationCore.Interfaces;
using NodaTime;

namespace ApplicationCore.Entities
{
    public abstract class AuditableEntity<TId> : IAuditableEntity<TId>
    {
        public TId Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = "?";
        public string UpdatedBy { get; set; } = "?";
        public string ExternalId { get; set; } = String.Empty;  
    }
}
