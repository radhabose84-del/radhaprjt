#nullable disable
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using UserManagement.Domain.Entities;
namespace BSOFT.Api.Configurations
{
    public static class JwtAuthenticationSetup
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            
            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
            {
                throw new ArgumentException("JWT SecretKey must be at least 32 characters long.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,                        
                        IssuerSigningKey =key,
                        ClockSkew = TimeSpan.Zero,
                        RoleClaimType = "role"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger("JwtAuthentication");

                            logger.LogWarning(context.Exception, "JWT authentication failed.");

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILoggerFactory>()
                                .CreateLogger("JwtAuthentication");

                            var userId = context.Principal?.Identity?.Name;
                            logger.LogInformation($"JWT token validated for user: {userId}");

                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();
            IdentityModelEventSource.ShowPII = true; // Enable detailed logs for JWT issues
        }
    }
}
