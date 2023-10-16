namespace ApplicationCore.Entities
{
    public class CustomAttribute : AuditableEntity<string>
    {
        public string EntityId { get; set; }
        public string CustomAttributeValue { get; set; }

        public string CustomAttributeDefinitionId { get; set; }
        public CustomAttributeDefinition CustomAttributeDefinition { get; set; }
    }
}
