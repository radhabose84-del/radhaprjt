using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.HttpResponse
{
    public class ApiResponseDTO<T>
    {
         public bool IsSuccess { get; set; }
         public string? Message { get; set; }
         public T? Data { get; set; }
         public int TotalCount { get; set;}
         public int PageNumber { get; set;}
         public int PageSize { get; set;}
        // public static ApiResponseDTO<T> Success(T data) => new ApiResponseDTO<T> { IsSuccess = true, Data = data };
        // public static ApiResponseDTO<T> Fail(string message) => new ApiResponseDTO<T> { IsSuccess = false, Message = message };

        // public object Where(Func<object, object> value)
        // {
        //     throw new NotImplementedException();
        // }
    }
}