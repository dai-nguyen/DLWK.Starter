using NodaTime;

namespace ApplicationCore.Models
{
    public class BaseResponse
    {
        public string Id { get; set; }
        public Instant DateCreated { get; set; }
        public Instant DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalId { get; set; }
    }

    
}
