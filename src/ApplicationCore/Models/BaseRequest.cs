namespace ApplicationCore.Models
{
    public class BaseCreateRequest
    {
        public string ExternalId { get; set; }
    }

    public class BaseUpdateRequest : BaseCreateRequest
    {
        public string Id { get; set; }        
    }
}
