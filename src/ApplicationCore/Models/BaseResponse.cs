namespace ApplicationCore.Models
{
    public class BaseResponse
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalId { get; set; }
    }

    
}
