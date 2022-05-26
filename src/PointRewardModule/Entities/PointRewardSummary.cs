using ApplicationCore.Entities;

namespace PointRewardModule.Entities
{
    public class PointRewardSummary : AuditableEntity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public int TotalPoint { get; set; }
    }
}
