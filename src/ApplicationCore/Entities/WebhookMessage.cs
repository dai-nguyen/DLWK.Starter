namespace ApplicationCore.Entities
{
    public class WebhookMessage : AuditableEntity<string>
    {        
        public string EntityId { get; set; }        
        public int FailedCount { get; set; }
        public DateTime? LastTryDate { get; set; }
        public bool? IsOkResponse { get; set; }

        public string SubscriberId { get; set; }
        public virtual WebhookSubscriber Subscriber { get; set; }
    }
}
