// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Contracts.Common;

// public class Result<T>
// {
//     public bool IsSuccess { get; }
//     public T Value { get; }
//     public Error Error { get; }

//     private Result(bool isSuccess, T value, Error error)
//     {
//         IsSuccess = isSuccess;
//         Value = value;
//         Error = error;
//     }

//     public static Result<T> Success(T value) => new(true, value, Error.None);
//     public static Result<T> Failure(Error error) => new(false, default!, error);
// }

// public record Error(string Code, string Message)
// {
//     public static readonly Error None = new(string.Empty, string.Empty);
//     public static Error NotFound(string entity, object id) =>
//         new("NotFound", $"{entity} with id {id} was not found");
//     public static Error Validation(string message) =>
//         new("Validation", message);
// }
