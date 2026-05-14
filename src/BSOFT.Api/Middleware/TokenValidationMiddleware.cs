using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUserSession;

namespace BSOFT.Api.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;
    private readonly IHostEnvironment _env;

    // D7 — only update session LastActivity if idle longer than this threshold
    private static readonly TimeSpan SessionUpdateInterval = TimeSpan.FromMinutes(1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // D6 — constants replace magic strings; all downstream services must use these
    public static class ContextKeys
    {
        public const string UserId   = "UserId";
        public const string UserName = "UserName";
    }

    public TokenValidationMiddleware(
        RequestDelegate next,
        ILogger<TokenValidationMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task Invoke(
        HttpContext context,
        IJwtTokenHelper jwtTokenHelper,
        IUserSessionRepository sessionRepository,
        ITimeZoneService timeZoneService)
    {
        // D4 — bypass checks FIRST before any timezone or DB work
        var path = context.Request.Path;

        if (path.StartsWithSegments("/notificationHub", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers.Authorization
            .FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("TraceId: {TraceId} | Missing token at {Path}",
                context.TraceIdentifier, path);
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized,
                "Authorization token is missing.");
            return;
        }

        try
        {
            var principal = jwtTokenHelper.ValidateToken(token);

            var jti = principal.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                _logger.LogWarning("TraceId: {TraceId} | Token missing JTI claim at {Path}",
                    context.TraceIdentifier, path);
                await WriteErrorResponse(context, StatusCodes.Status401Unauthorized,
                    "Invalid token.");
                return;
            }

            // D4 — timezone resolved here, only for authenticated requests
            var currentTime = timeZoneService.GetCurrentTime(
                timeZoneService.GetSystemTimeZone());

            var session = await sessionRepository.GetSessionByJwtIdAsync(jti);
            if (session is null || session.IsActive == 0 || session.ExpiresAt <= currentTime)
            {
                _logger.LogWarning("TraceId: {TraceId} | Invalid or expired session JTI: {Jti}",
                    context.TraceIdentifier, jti);
                await WriteErrorResponse(context, StatusCodes.Status401Unauthorized,
                    "Session is invalid or expired. Please login again.");
                return;
            }

            // D6 — use constants
            context.Items[ContextKeys.UserId]   = principal.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            context.Items[ContextKeys.UserName] = principal.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

            context.User = principal;

            // D7 — only write to DB if idle longer than 1 minute
            if (currentTime - session.LastActivity > SessionUpdateInterval)
            {
                session.LastActivity = currentTime;
                await sessionRepository.UpdateSessionAsync(session);
            }
        }
        catch (SecurityTokenExpiredException)
        {
            // D3 — specific catch, no internal detail exposed
            _logger.LogWarning("TraceId: {TraceId} | Expired token at {Path}",
                context.TraceIdentifier, path);
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized,
                "Token has expired. Please login again.");
            return;
        }
        catch (SecurityTokenException ex)
        {
            // D3 — env-aware: detail in dev, generic in prod
            _logger.LogWarning("TraceId: {TraceId} | Invalid token: {Message}",
                context.TraceIdentifier, ex.Message);
            var msg = _env.IsDevelopment() ? $"Invalid token: {ex.Message}" : "Invalid token.";
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, msg);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TraceId: {TraceId} | Token validation error at {Path}",
                context.TraceIdentifier, path);
            await WriteErrorResponse(context, StatusCodes.Status401Unauthorized,
                "Authentication failed.");
            return;
        }

        await _next(context);
    }

    // D5 — consistent ApiResponseDTO shape; D8 — RequestAborted cancellation
    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiResponseDTO<object>
        {
            IsSuccess  = false,
            StatusCode = statusCode,
            Message    = message,
            Errors     = [message],
            TraceId    = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions),
            context.RequestAborted);
    }
}
