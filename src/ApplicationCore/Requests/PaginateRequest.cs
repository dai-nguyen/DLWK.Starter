namespace ApplicationCore.Requests
{
    public class PaginateRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string OrderBy { get; set; } = "Id";
        public bool IsDescending { get; set; } = true;
    }
}
