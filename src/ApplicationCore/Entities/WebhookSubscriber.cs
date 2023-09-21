namespace ApplicationCore.Entities
{
    public class WebhookSubscriber : AuditableEntity<string>
    {        
        public string EntityName { get; set; }
        public string Operation { get; set; }
        public string Url { get; set; }
        public bool IsEnabled { get; set; }
        public int FailedCount { get; set; }

        public virtual IEnumerable<WebhookMessage> Messages { get; set; }
    }
}
