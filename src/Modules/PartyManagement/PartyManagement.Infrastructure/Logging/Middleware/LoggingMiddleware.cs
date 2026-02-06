using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Application.Common.Exceptions;
using PartyManagement.Application.Common.HttpResponse;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using FluentValidation;

namespace PartyManagement.Infrastructure.Logging.Middleware
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
            //     var traceId = context.TraceIdentifier;  // Generate a unique trace ID for each request            
            //     context.Request.EnableBuffering();
            //     try
            //     {
            //     // Log request details for POST, PUT, and GET requests
            //     if (context.Request.Method == HttpMethods.Post || 
            //         context.Request.Method == HttpMethods.Put || 
            //         context.Request.Method == HttpMethods.Get) // Include GET requests
            //         {
            //         var method = context.Request.Method;
            //         var path = context.Request.Path;

            //         if (method == HttpMethods.Get)
            //         {
            //             _logger.LogInformation("TraceId: {TraceId}, Method: {Method}, Path: {Path}", traceId, method, path);
            //         }
            //         else
            //         {
            //             context.Request.Body.Position = 0;
            //             using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            //             var body = await reader.ReadToEndAsync();
            //             context.Request.Body.Position = 0;

            //             _logger.LogInformation("TraceId: {TraceId}, Method: {Method}, Path: {Path}, Body: {Body}", traceId, method, path, body);
            //         }
            //     }
            //     await _next(context);                    
            // }
            // catch (Exception ex)
            //     {
            //         // 🛑 Log and return standardized error
            //         _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

            //         context.Response.ContentType = "application/json";
            //         context.Response.StatusCode = ex switch
            //         {
            //             EntityAlreadyExistsException => StatusCodes.Status409Conflict,
            //             EntityNotFoundException => StatusCodes.Status404NotFound,
            //             ExceptionRules => StatusCodes.Status400BadRequest,
            //             SqlException => StatusCodes.Status503ServiceUnavailable,
            //             DbUpdateException => StatusCodes.Status500InternalServerError,
            //             NullReferenceException => StatusCodes.Status500InternalServerError,
            //             _ => StatusCodes.Status500InternalServerError
            //         };

            //         var response =  new ApiResponseDTO<object>
            //         {
            //             IsSuccess = false,
            //             Message = ex.Message,                    
            //            /*  Data = ex switch
            //             {
            //                 EntityAlreadyExistsException => "Already exists.",
            //                 EntityNotFoundException => "Id not found.",
            //                 ExceptionRules => "Validation error.",
            //                 SqlException => "Database connection error.",
            //                 DbUpdateException => "Database update failed.",
            //                 NullReferenceException => "Null reference exception.",
            //                 _ => "An unexpected error occurred."
            //             }, */
            //             Errors = new List<string> { ex.Message },
            //             StatusCode = context.Response.StatusCode
            //         };
            //         await context.Response.WriteAsJsonAsync(response);
            //     }
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
                // Handle any exceptions that occur
                _logger.LogError(ex, "Unhandled exception");

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
    }
}
