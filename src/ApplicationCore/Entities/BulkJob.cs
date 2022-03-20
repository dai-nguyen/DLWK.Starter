namespace ApplicationCore.Entities
{
    public class BulkJob : AuditableEntity<string>
    {
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int Processed { get; set; }
        public int Failed { get; set; }
    }
}
