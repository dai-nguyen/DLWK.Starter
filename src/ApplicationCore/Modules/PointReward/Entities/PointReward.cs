using ApplicationCore.Entities;
using NpgsqlTypes;

namespace ApplicationCore.Modules.PointReward.Entities
{
    public class PointReward : AuditableEntity<string>
    {
        public string UserId { get; set; } = string.Empty;
        public int Point { get; set; }
        public string Notes { get; set; } = string.Empty;

        public NpgsqlTsVector SearchVector { get; set; }
    }
}
