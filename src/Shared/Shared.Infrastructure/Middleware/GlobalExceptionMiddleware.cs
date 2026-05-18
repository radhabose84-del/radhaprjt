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

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task Invoke(HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        try
        {
            await _next(context);
        }
        catch (TaskCanceledException)
        {
            // Client disconnected — no response needed
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — no response needed
        }
        catch (ValidationException ex)
        {
            _logger.LogDebug("TraceId: {TraceId} | Validation failed: {Errors}",
                traceId, string.Join(" | ", ex.Errors.Select(e => e.ErrorMessage)));

            await HandleValidationException(context, ex, traceId);
        }
        catch (EntityAlreadyExistsException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Already exists: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status409Conflict, ex.Message, traceId);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Not found: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status404NotFound, ex.Message, traceId);
        }
        catch (ExceptionRules ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Business rule: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status400BadRequest, ex.Message, traceId);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Unauthorized: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status401Unauthorized, ex.Message, traceId);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Null argument: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status400BadRequest, ex.Message, traceId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Bad argument: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status400BadRequest, ex.Message, traceId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Key not found: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status404NotFound, ex.Message, traceId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("TraceId: {TraceId} | Invalid operation: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status400BadRequest, ex.Message, traceId);
        }
        catch (NotImplementedException ex)
        {
            _logger.LogError("TraceId: {TraceId} | Not implemented: {Message}", traceId, ex.Message);
            await HandleException(context, StatusCodes.Status501NotImplemented, "Feature not implemented.", traceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TraceId: {TraceId} | Unhandled exception at {Path}",
                traceId, context.Request.Path);

            var message = _env.IsDevelopment()
                ? $"{ex.Message}{(ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "")}"
                : "An unexpected error occurred. Please contact support.";

            await HandleException(context, StatusCodes.Status500InternalServerError, message, traceId);
        }
    }

    private static async Task HandleValidationException(
        HttpContext context, ValidationException ex, string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = StatusCodes.Status400BadRequest;

        var errors = (ex.Errors ?? [])
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct()
            .ToList();

        if (errors.Count == 0)
            errors = [ex.Message];

        var response = new ApiResponseDTO<object>
        {
            IsSuccess  = false,
            StatusCode = StatusCodes.Status400BadRequest,
            Message    = "Validation failed",
            Errors     = errors,
            TraceId    = traceId
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions),
            context.RequestAborted);
    }

    private static async Task HandleException(
        HttpContext context, int statusCode, string message, string traceId)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = statusCode;

        var response = new ApiResponseDTO<object>
        {
            IsSuccess  = false,
            StatusCode = statusCode,
            Message    = message,
            Errors     = [message],
            TraceId    = traceId
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions),
            context.RequestAborted);
    }
}
