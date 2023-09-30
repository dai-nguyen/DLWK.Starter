using ApplicationCore.Enums;
using NodaTime;
using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Project : AuditableEntity<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public Instant DateStart { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public Instant DateDue { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public NpgsqlTsVector SearchVector { get; set; }

        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string ContactId { get; set; }
        public virtual Contact Contact { get; set; }
    }
}
