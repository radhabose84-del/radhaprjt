namespace BSOFT.Api.Middleware;

/// <summary>
/// Replaces non-breaking spaces (U+00A0 / 0xC2 0xA0) and other invisible Unicode whitespace
/// with regular spaces in JSON request bodies to prevent System.Text.Json deserialization failures.
/// </summary>
public sealed class JsonBodySanitizingMiddleware
{
    private readonly RequestDelegate _next;

    public JsonBodySanitizingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsJsonRequest(context.Request) && (context.Request.ContentLength ?? 0) > 0)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: false);
            var body = await reader.ReadToEndAsync();

            // Replace non-breaking space (U+00A0 / 0xC2 0xA0) with regular space
            var sanitized = body.Replace('\u00A0', ' ');

            // Strip BOM (U+FEFF) if present
            if (sanitized.Length > 0 && sanitized[0] == '\uFEFF')
            {
                sanitized = sanitized[1..];
            }

            var sanitizedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sanitized));
            context.Request.Body = sanitizedStream;
            context.Request.ContentLength = sanitizedStream.Length;
        }

        await _next(context);
    }

    private static bool IsJsonRequest(HttpRequest request)
    {
        return request.ContentType != null
            && request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
