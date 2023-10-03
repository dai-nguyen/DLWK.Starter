using ApplicationCore.Entities;
using ApplicationCore.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using NodaTime;

namespace ApplicationCore.Models
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new();
        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public Audit ToAudit()
        {
            var audit = new Audit()
            {
                UserId = UserId,
                Type = AuditType.ToString(),
                TableName = TableName,
                DateTime = SystemClock.Instance.GetCurrentInstant(),
                PrimaryKey = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? "" : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? "" : JsonConvert.SerializeObject(NewValues),
                AffectedColumns = ChangedColumns.Count == 0 ? "" : JsonConvert.SerializeObject(ChangedColumns)
            };
            return audit;
        }
    }
}
