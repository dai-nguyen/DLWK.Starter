namespace ApplicationCore.Models
{
    public enum BulkOperation
    {
        Upsert,
        Delete
    }

    public class BulkModelBase
    {
        public string ExternalId { get; set; } = string.Empty;
        public BulkOperation Operation { get; set; }
    }

    public class BulkResponseBase
    {
        public IEnumerable<BulkMessageResponse> Messages = Enumerable.Empty<BulkMessageResponse>();
        public int Processed { get; set; }
        public int Failed { get; set; }
    }

    public class BulkMessageResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Request { get; set; }= string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }
}
