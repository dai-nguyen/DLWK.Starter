using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class ContactUdDefinition : AuditableEntity<string>
    {
        public string Label { get; set; }
        public string Code { get; set; }
        public UserDefinedDataType DataType { get; set; }
        public string[] DropdownValues { get; set; }
    }
}
