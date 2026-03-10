using System.Text.Json;
using Contracts.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        context.Request.EnableBuffering();

        try
        {
            LogRequest(context, traceId);
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationException(context, ex);
        }
        catch (EntityNotFoundException ex)
        {
            await HandleException(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ExceptionRules ex)
        {
            await HandleException(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TraceId: {TraceId}, Unhandled exception", traceId);

            var message = _env.IsDevelopment()
                ? $"{ex.Message}{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "")}"
                : "Internal Server Error";

            await HandleException(context, StatusCodes.Status500InternalServerError, message);
        }
    }

    private void LogRequest(HttpContext context, string traceId)
    {
        var method = context.Request.Method;
        if (method == HttpMethods.Get || method == HttpMethods.Post || method == HttpMethods.Put)
        {
            _logger.LogInformation("TraceId: {TraceId}, Request Path: {Path}, Method: {Method}",
                traceId, context.Request.Path, context.Request.Method);
        }
    }

    private static async Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = (ex.Errors ?? Enumerable.Empty<FluentValidation.Results.ValidationFailure>())
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();

        if (errors.Count == 0)
            errors = [ex.Message];

        var response = new ApiResponseDTO<object>
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed",
            Errors = errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static async Task HandleException(HttpContext context, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiResponseDTO<object>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
