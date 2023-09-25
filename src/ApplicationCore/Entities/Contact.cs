using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Contact : AuditableEntity<string>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

        public string CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public virtual IEnumerable<Project> Projects { get; set; }
    }
}
