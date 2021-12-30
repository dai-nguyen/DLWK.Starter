namespace ApplicationCore.Entities
{
    public class DocumentType : AuditableEntity<string>
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
