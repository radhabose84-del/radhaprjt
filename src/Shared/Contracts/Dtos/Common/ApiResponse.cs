namespace Contracts.Dtos.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse(T? data)
        {
            IsSuccess = true;
            Message = "Success";
            Data = data;
            TotalCount = 0;
            PageNumber = 0;
            PageSize = 0;
            StatusCode = 200;
        }

        public ApiResponse(int statusCode, string message, T? data)
        {
            StatusCode = statusCode;
            IsSuccess = statusCode >= 200 && statusCode < 300;
            Message = message;
            Data = data;
        }
    }
}