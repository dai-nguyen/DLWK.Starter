using ApplicationCore.Entities;

namespace PointRewardModule.Entities
{
    public class Bank : AuditableEntity<string>
    {        
        public string OwnerId { get; set; } = string.Empty;
        public string BankType { get; set; } = BankTypes.Checking;

        public int Balance { get; set; }

        public IEnumerable<Transaction> Transactions { get; set; }
    }

    public static class BankTypes
    {
        public const string Checking = "Checking";
        public const string Saving = "Saving";
        public const string Investing = "Investing";
    }
}
