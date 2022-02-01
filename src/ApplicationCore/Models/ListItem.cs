namespace ApplicationCore.Models
{
    public class ListItem
    {
        public string Display { get; set; }
        public string Value { get; set; }
        
        public ListItem()
        {
            Display = string.Empty;
            Value = string.Empty;            
        }

        public ListItem(
            string display, 
            string value)
        {
            Display = display;
            Value = value;            
        }
    }
}
