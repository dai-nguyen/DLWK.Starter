namespace ApplicationCore.Models
{
    public class CreateRequestBase
    {
        public string ExternalId { get; set; }
    }

    public class UpdateRequestBase : CreateRequestBase
    {
        public string? Id { get; set; }        
    }
}
