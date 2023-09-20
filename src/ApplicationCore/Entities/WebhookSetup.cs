namespace ApplicationCore.Entities
{
    public class WebhookSetup : AuditableEntity<string>
    {
        public string Url { get; set; }
        public string EntityName { get; set; }
        public string Operation { get; set; }
        public bool IsEnabled { get; set; }
        public int FailCount { get; set; }
    }
}
