using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Document : AuditableEntity<string>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;        
        public string Data { get; set; } = string.Empty;

        public NpgsqlTsVector SearchVector { get; set; }

        public string DocumentTypeId { get; set; } = string.Empty;
        public virtual DocumentType DocumentType { get; set; }
    }
}
