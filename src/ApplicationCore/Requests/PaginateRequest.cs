namespace ApplicationCore.Requests
{
    public class PaginateRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; } = "Id";
        public bool IsDescending { get; set; } = true;
    }
}
