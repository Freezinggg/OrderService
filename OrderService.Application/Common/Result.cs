using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Common
{
    public class Result<T>
    {
        public T? Data { get; set; }
        public ResultStatus Status { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccess => Status == ResultStatus.Success;
        public bool IsNotFound => Status == ResultStatus.NotFound;
        public bool IsError => Status == ResultStatus.Error;
        public bool IsInvalid => Status == ResultStatus.Invalid;
        public bool IsServiceUnavailable => Status == ResultStatus.ServiceUnavailable;
        public bool IsFail => Status == ResultStatus.Fail;

        public static Result<T> Success(T data) =>
            new() { Status = ResultStatus.Success, Data = data };

        public static Result<T> Fail(string? message = null) =>
            new() { Status = ResultStatus.Fail, ErrorMessage = message };

        public static Result<T> NotFound(string? message = null) =>
            new() { Status = ResultStatus.NotFound, ErrorMessage = message };

        public static Result<T> Error(string? message = null) =>
            new() { Status = ResultStatus.Error, ErrorMessage = message };
        public static Result<T> Invalid(string? message = null) =>
            new() { Status = ResultStatus.Invalid, ErrorMessage = message };
        public static Result<T> ServiceUnavailable(string? message = null) =>
            new() { Status = ResultStatus.ServiceUnavailable, ErrorMessage = message };
    }

    public enum ResultStatus : int
    {
        Success,
        NotFound,
        Error,
        Invalid,
        ServiceUnavailable,
        Fail,
    }
}
