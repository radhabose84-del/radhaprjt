namespace Contracts.Dtos.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        // ✅ Add this constructor
        public ApiResponse(T data)
        {
            IsSuccess = true;
            Message = "Success";
            Data = data;
            TotalCount = 0;
            PageNumber = 0;
            PageSize = 0;
            StatusCode = 200;
        }
    }
}