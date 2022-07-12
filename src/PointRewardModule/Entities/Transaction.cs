using ApplicationCore.Entities;
using NpgsqlTypes;

namespace PointRewardModule.Entities
{
    public class Transaction : AuditableEntity<string>
    {
        public string BankId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Notes { get; set; } = string.Empty;

        public NpgsqlTsVector SearchVector { get; set; }

        public Bank Bank { get; set; }
    }
}
