namespace ApplicationCore.Entities
{
    public class WebhookItem : AuditableEntity<string>
    {        
        public string EntityId { get; set; }        
        public int FailCount { get; set; }
        public DateTime? LastTryDate { get; set; }
        public string WebhookSetupId { get; set; }
    }
}
