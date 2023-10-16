using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class CustomAttributeDefinition : AuditableEntity<string>
    {
        public string EntityName { get; set; }        
        public string AttributeLabel { get; set; }
        public string AttributeCode { get; set; }
        public CustomAttributeType AttributeType { get; set; }
        public string[] DropdownValues { get; set; }

        public virtual IEnumerable<CustomAttribute> CustomAttributes { get; set; }
    }
}
