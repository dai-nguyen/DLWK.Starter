using ApplicationCore.Entities;

namespace ApplicationCore.Modules.PointReward.Entities
{
    public class PointRewardSummary : AuditableEntity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalPoint { get; set; }
    }
}
