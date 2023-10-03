using ApplicationCore.Interfaces;
using NodaTime;

namespace ApplicationCore.Entities
{
    public class Audit : IEntity<string>
    {
        public string Id { get; set; } = new Guid().ToString();
        public string UserId { get; set; } = "";
        public string Type { get; set; } = "";
        public string TableName { get; set; } = "";
        public Instant DateTime { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public string OldValues { get; set; } = "";
        public string NewValues { get; set; } = "";
        public string AffectedColumns { get; set; } = "";
        public string PrimaryKey { get; set; } = "";
    }
}
