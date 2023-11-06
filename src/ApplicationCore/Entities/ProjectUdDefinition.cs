using ApplicationCore.Enums;

namespace ApplicationCore.Entities
{
    public class ProjectUdDefinition : AuditableEntity<string>
    {
        public string Label { get; set; }
        public string Code { get; set; }
        public UserDefinedDataType DataType { get; set; }
        public string[] DropdownValues { get; set; }
    }
}
