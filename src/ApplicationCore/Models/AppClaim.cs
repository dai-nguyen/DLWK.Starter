namespace ApplicationCore.Models
{
    public class AppClaim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public AppClaim()
        {
            Type = string.Empty;
            Value = string.Empty;
        }

        public AppClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
