using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Infrastructure.Logging.Middleware
{
    public sealed class LoggingMiddleware
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

            // ✅ enable buffering so body can be read here + later by MVC
            context.Request.EnableBuffering();

            string requestBody = "";
            if (context.Request.Method is "POST" or "PUT" or "PATCH")
            {
                requestBody = await ReadRequestBodySafeAsync(context);
            }

            // ✅ log request (keep GET minimal)
            if (context.Request.Method == HttpMethods.Get)
            {
                _logger.LogInformation("TraceId: {TraceId}, {Method} {Path}{QueryString}",
                    traceId, context.Request.Method, context.Request.Path, context.Request.QueryString);
            }
            else if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Patch)
            {
                _logger.LogInformation("TraceId: {TraceId}, {Method} {Path}, Body: {Body}",
                    traceId, context.Request.Method, context.Request.Path, requestBody);
            }

            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                var errors = (ex.Errors ?? Enumerable.Empty<FluentValidation.Results.ValidationFailure>())
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct()
                    .ToArray();

                if (errors.Length == 0)
                    errors = new[] { ex.Message };

                _logger.LogWarning(ex,
                    "TraceId: {TraceId}, Validation failed for {Method} {Path}. Errors: {Errors}",
                    traceId, context.Request.Method, context.Request.Path, errors);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "Validation failed",
                    errors: errors,
                    traceId: traceId
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex,
                    "TraceId: {TraceId}, Not found for {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status404NotFound,
                    message: "The requested resource could not be found.",
                    errors: new[] { ex.Message },
                    traceId: traceId
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex,
                    "TraceId: {TraceId}, SQL error for {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    message: "Unable to connect to the database. Please try again later.",
                    errors: new[] { ex.Message },
                    traceId: traceId
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "TraceId: {TraceId}, DB update error for {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "A database update error occurred.",
                    errors: new[] { ex.InnerException?.Message ?? ex.Message },
                    traceId: traceId
                );
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex,
                    "TraceId: {TraceId}, Null reference for {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "A null reference occurred.",
                    errors: new[] { ex.InnerException?.Message ?? ex.Message },
                    traceId: traceId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "TraceId: {TraceId}, Unhandled error for {Method} {Path}",
                    traceId, context.Request.Method, context.Request.Path);

                await WriteProblemJsonSafeAsync(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred.",
                    errors: new[] { ex.InnerException?.Message ?? ex.Message },
                    traceId: traceId
                );
            }
        }

        private static async Task<string> ReadRequestBodySafeAsync(HttpContext context)
        {
            try
            {
                if (context.Request.ContentLength is null or 0)
                    return "";

                context.Request.Body.Position = 0;

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();

                // ✅ reset so MVC can read it
                context.Request.Body.Position = 0;

                return body;
            }
            catch
            {
                // never fail logging
                try { context.Request.Body.Position = 0; } catch { }
                return "[Body read failed]";
            }
        }

        private static async Task WriteProblemJsonSafeAsync(
            HttpContext context,
            int statusCode,
            string message,
            string[] errors,
            string traceId)
        {
            // If response already started, we cannot change headers/body safely
            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = new
            {
                traceId,
                statusCode,
                message,
                errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
