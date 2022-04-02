using NpgsqlTypes;

namespace ApplicationCore.Entities
{
    public class Document : AuditableEntity<string>
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;        
        public string Data { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;

        public NpgsqlTsVector SearchVector { get; set; }
    }
}
