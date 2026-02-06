// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text.Json;
// using UserManagement.Application.Common.Interfaces;
// using UserManagement.Application.Common.Interfaces.IUserSession;
// using UserManagement.Domain.Entities;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Options;

// namespace UserManagement.API.Middleware
// {   
//     public class TokenValidationMiddleware
//     {
//         private readonly RequestDelegate _next;
//         private readonly JwtSettings _jwtSettings;
//         private readonly ITimeZoneService _timeZoneService;

//         public TokenValidationMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings, ITimeZoneService timeZoneService)
//         {
//             _next = next;
//             _jwtSettings = jwtSettings.Value;
//             _timeZoneService = timeZoneService;
//         }

//         public async Task Invoke(HttpContext context, IJwtTokenHelper jwtTokenHelper, IUserSessionRepository sessionRepository)
//         {
//             var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
//             var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);             
//             var endpoint = context.GetEndpoint();
//             if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
//             {
//                 await _next(context);
//                 return;
//             }

//             // Check for token in the Authorization header
//             var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
//             if (string.IsNullOrEmpty(token))
//             {
//                 await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, "Authorization token is missing.");
//                 return;
//             }

//             try
//             {
//                 // Validate the token
//                 var principal = jwtTokenHelper.ValidateToken(token);

//                 // Extract the JWT ID (jti) and validate it
//                 var jti = principal.Claims.FirstOrDefault(c => c.Type is JwtRegisteredClaimNames.Jti)?.Value;
//                 if (string.IsNullOrEmpty(jti))
//                 {
//                     await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, "Invalid JWT ID.");
//                     return;
//                 }

//                 // Check session in the database
//                 var session = await sessionRepository.GetSessionByJwtIdAsync(jti);
//                 if (session is null || session.IsActive is 0 || session.ExpiresAt <= currentTime)
//                 {
//                     await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, "Session is invalid or expired.");
//                     return;
//                 }

//                 // Attach UserId and UserName to HttpContext for further use
//                 context.Items["UserId"] = principal.Claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier)?.Value;
//                 context.Items["UserName"] = principal.Claims.FirstOrDefault(c => c.Type is JwtRegisteredClaimNames.Name)?.Value;

//                 // Update session's last activity
//                 session.LastActivity = currentTime;
//                 await sessionRepository.UpdateSessionAsync(session);

//                 // Set the User principal
//                 context.User = principal;
//             }
//             catch (Exception ex)
//             {
//                 await WriteErrorResponse(context, StatusCodes.Status401Unauthorized, $"Invalid token: {ex.Message}");
//                 return;
//             }

//             await _next(context);
//         }

//         private async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
//         {
//             context.Response.StatusCode = statusCode;
//             context.Response.ContentType = "application/json";

//             var errorResponse = new
//             {
//                 StatusCode = statusCode,
//                 Message = message
//             };

//             await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
//         }
//     }
// }
