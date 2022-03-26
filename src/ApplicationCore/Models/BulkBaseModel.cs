using ApplicationCore.Interfaces;

namespace ApplicationCore.Models
{
    public enum BulkOperation
    {
        Upsert,
        Delete
    }

    public class BulkBaseModel
    {
        public string ExternalId { get; set; }
        public BulkOperation Operation { get; set; }
    }
}
