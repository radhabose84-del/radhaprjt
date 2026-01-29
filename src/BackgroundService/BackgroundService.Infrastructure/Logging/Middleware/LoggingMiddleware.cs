using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Logging.Middleware
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
            var traceId = context.TraceIdentifier;
            context.Request.EnableBuffering();

            try
            {

                if (context.Request.Method == HttpMethods.Post ||
                    context.Request.Method == HttpMethods.Put ||
                    context.Request.Method == HttpMethods.Get)
                {

                    if (context.Request.Method == HttpMethods.Get)
                    {
                        _logger.LogInformation("TraceId: {TraceId}, Request Path: {Path}, Method: {Method}",
                            traceId, context.Request.Path, context.Request.Method);
                    }
                    else
                    {

                        context.Request.Body.Position = 0;
                        var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                        _logger.LogInformation("TraceId: {TraceId}, Request Path: {Path}, Method: {Method}, Body: {Body}",
                            traceId, context.Request.Path, context.Request.Method, requestBody);
                        context.Request.Body.Position = 0;
                    }
                }


                await _next(context);
            }
            catch (ValidationException ex)
            {

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

                _logger.LogError(ex, "Unhandled exception");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var response = new
                {
                    statusCode = context.Response.StatusCode,
                    message = "Internal Server Error",
                    errors = new[] { ex.Message }
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            
        }
    }
}