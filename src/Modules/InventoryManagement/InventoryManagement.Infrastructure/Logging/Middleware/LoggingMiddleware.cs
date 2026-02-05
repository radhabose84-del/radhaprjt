using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.HttpResponse;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using FluentValidation;


namespace PurchaseManagement.Infrastructure.Logging.Middleware
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
                var method = context.Request.Method;
                var path = context.Request.Path;

                if (method == HttpMethods.Get)
                {
                    _logger.LogInformation("TraceId: {TraceId}, Method: {Method}, Path: {Path}", traceId, method, path);
                }
                else
                {
                    context.Request.Body.Position = 0;
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    _logger.LogInformation("TraceId: {TraceId}, Method: {Method}, Path: {Path}, Body: {Body}", traceId, method, path, body);
                }
            }
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
