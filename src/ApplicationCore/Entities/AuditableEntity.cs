using ApplicationCore.Interfaces;
using NodaTime;

namespace ApplicationCore.Entities
{
    public abstract class AuditableEntity<TId> : IAuditableEntity<TId>
    {
        public TId Id { get; set; }
        public Instant DateCreated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public Instant DateUpdated { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public string CreatedBy { get; set; } = "?";
        public string UpdatedBy { get; set; } = "?";
        public string ExternalId { get; set; } = String.Empty;  
    }
}
