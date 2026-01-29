
namespace BSOFT.Bootstrapper.Configurations
{
    public static class CorsSetup
    {
        public static void AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin() // In production: replace with .WithOrigins("https://your-frontend.com")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }
}