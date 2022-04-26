using ApplicationCore.Entities;

namespace ApplicationCore.Modules.PointReward.Entities
{
    public class PointReward : AuditableEntity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public int Point { get; set; }
    }
}
