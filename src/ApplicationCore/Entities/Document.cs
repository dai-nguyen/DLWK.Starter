using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Document : AuditableEntity<string>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; } = false;
        public string URL { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }

        public string DocumentTypeId { get; set; }
        public virtual DocumentType DocumentType { get; set; }
    }
}
