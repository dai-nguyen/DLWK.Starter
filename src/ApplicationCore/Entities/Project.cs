using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Project : AuditableEntity<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateDue { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

        public string CustomerId { get; set; }
        public string ContactId { get; set; }
    }
}
