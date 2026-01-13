using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null) => new ApiResponse<T> { Success = true, Data = data, Message = message };
        public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}
