using ApplicationCore.Models;
using NodaTime;

namespace ApplicationCore.Interfaces
{
    public interface IEntity
    {

    }

    public interface IEntity<TId> : IEntity
    {
        TId Id { get; set; }        
    }    

    

    public interface IAuditableEntity : IEntity
    {
        Instant DateCreated { get; set; }
        Instant DateUpdated { get; set; }
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
        string ExternalId { get; set; }
    }

    public interface IAuditableEntity<TId> : IAuditableEntity, IEntity<TId>
    {

    }

    public interface IAuditableCustomAttributeEntity : IAuditableEntity
    {
        //IEnumerable<CustomAttribute> CustomAttributes { get; set; }
    }

    public interface IAuditableCustomAttributeEntity<TId> : IAuditableCustomAttributeEntity, IEntity<TId>
    {

    }
}
