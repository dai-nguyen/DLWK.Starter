using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace ApplicationCore.Entities
{
    public abstract class AuditableCustomAttributeEntity<TId> : AuditableEntity<TId>, IAuditableCustomAttributeEntity<TId>
    {
        public TId Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalId { get; set; }        

        //public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
    }
}
