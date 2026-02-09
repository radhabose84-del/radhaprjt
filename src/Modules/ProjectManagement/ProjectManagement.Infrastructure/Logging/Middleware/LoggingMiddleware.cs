using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace ProjectManagement.Infrastructure.Logging.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var traceId = context.TraceIdentifier;  // Generate a unique trace ID for each request            
            context.Request.EnableBuffering();

            try
            {
                // Log request details for POST, PUT, and GET requests
                if (context.Request.Method == HttpMethods.Post || 
                    context.Request.Method == HttpMethods.Put || 
                    context.Request.Method == HttpMethods.Get) // Include GET requests
                {
                    // For GET requests, there's no body, but you can log the request path and other details
                    if (context.Request.Method == HttpMethods.Get)
                    {
                        _logger.LogInformation("TraceId: {TraceId}, Request Path: {Path}, Method: {Method}", 
                            traceId, context.Request.Path, context.Request.Method);
                    }
                    else
                    {
                        // For POST and PUT requests, we log the body along with path and method
                        context.Request.Body.Position = 0;  // Reset position before reading
                        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                        _logger.LogInformation("TraceId: {TraceId}, Request Path: {Path}, Method: {Method}, Body: {Body}", 
                            traceId, context.Request.Path, context.Request.Method, requestBody);
                        context.Request.Body.Position = 0;  // Reset position after reading
                    }
                }

                // Call the next middleware in the pipeline
                await _next(context);
            }
             catch (ValidationException ex)
            {
                // Handle 400 - Validation Error
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
    
                  var errors = (ex.Errors ?? Enumerable.Empty<FluentValidation.Results.ValidationFailure>())
                .Select(e => e.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToArray();

                if (errors.Length == 0)
                errors = new[] { ex.Message };
    
                var response = new
                {
                    statusCode = context.Response.StatusCode,
                    message = "Validation failed",
                    errors 
                };
    
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                 int statusCode;
            string title;
            string detail;

            // Determine error details based on exception type
            switch (ex)
            {
                case KeyNotFoundException keyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound; // Use 404 for resource not found
                    title = "The requested resource could not be found.";
                    detail =keyNotFoundException.Message;
                    _logger.LogError(keyNotFoundException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
                        statusCode, title, context.Request.Path);
                    break;

                case DbUpdateException dbUpdateException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "A database update error occurred.";
                    detail = ex.Message;
                    _logger.LogError(dbUpdateException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
                        statusCode, title, context.Request.Path);
                    break;

                case SqlException sqlException:
                    statusCode = StatusCodes.Status503ServiceUnavailable; // Indicate service unavailability
                    title = "Unable to connect to the database. Please try again later.";
                    detail = ex.Message;
                    _logger.LogError(sqlException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
                        statusCode, title, context.Request.Path);
                    break;

                case NullReferenceException nullReferenceException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "A null reference occurred.";
                    detail = nullReferenceException.InnerException?.Message ?? nullReferenceException.Message;
                    _logger.LogError(nullReferenceException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
                        statusCode, title, context.Request.Path);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "An unexpected error occurred.";
                    detail = ex.InnerException?.Message ?? ex.Message;
                    _logger.LogError(ex, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
                        statusCode, title, context.Request.Path);
                    break;
            }

                 context.Response.ContentType = "application/json";
                 context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                 var response = new
                 {
                     statusCode = context.Response.StatusCode,
                     message = "Internal Server Error",
                     errors = new[] { ex.Message } // optional: ex.StackTrace for development
                 };

                 await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        // private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        // {
        //     int statusCode;
        //     string title;
        //     string detail;

        //     // Determine error details based on exception type
        //     switch (exception)
        //     {
        //         case KeyNotFoundException keyNotFoundException:
        //             statusCode = StatusCodes.Status404NotFound; // Use 404 for resource not found
        //             title = "The requested resource could not be found.";
        //             detail =keyNotFoundException.Message;
        //             _logger.LogError(keyNotFoundException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
        //                 statusCode, title, context.Request.Path);
        //             break;

        //         case DbUpdateException dbUpdateException:
        //             statusCode = StatusCodes.Status500InternalServerError;
        //             title = "A database update error occurred.";
        //             detail = exception.Message;
        //             _logger.LogError(dbUpdateException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
        //                 statusCode, title, context.Request.Path);
        //             break;

        //         case SqlException sqlException:
        //             statusCode = StatusCodes.Status503ServiceUnavailable; // Indicate service unavailability
        //             title = "Unable to connect to the database. Please try again later.";
        //             detail = exception.Message;
        //             _logger.LogError(sqlException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
        //                 statusCode, title, context.Request.Path);
        //             break;

        //         case NullReferenceException nullReferenceException:
        //             statusCode = StatusCodes.Status500InternalServerError;
        //             title = "A null reference occurred.";
        //             detail = nullReferenceException.InnerException?.Message ?? nullReferenceException.Message;
        //             _logger.LogError(nullReferenceException, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
        //                 statusCode, title, context.Request.Path);
        //             break;

        //         default:
        //             statusCode = StatusCodes.Status500InternalServerError;
        //             title = "An unexpected error occurred.";
        //             detail = exception.InnerException?.Message ?? exception.Message;
        //             _logger.LogError(exception, "Error Code: {ErrorCode}, Message: {Message}, Path: {Path}",
        //                 statusCode, title, context.Request.Path);
        //             break;
        //     }

        //     // Set the response
        //     context.Response.ContentType = "application/json";
        //     context.Response.StatusCode = statusCode;

        //     // Generate the error response
        //     var errorResponse = new
        //     {
        //         TraceId = context.TraceIdentifier,
        //         StatusCode = statusCode,
        //         Title = title,
        //         Detail = detail
        //     };

        //     await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        // }
    }
}
