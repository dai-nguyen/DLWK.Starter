using ApplicationCore.Interfaces;

namespace ApplicationCore.Models
{
    public enum BulkOperation
    {
        Upsert,
        Delete
    }

    public class BulkModelBase
    {
        public string ExternalId { get; set; }
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
        public string Request { get; set; }
        public string Operation { get; set; }        
        public string Response { get; set; }
    }
}
