#nullable disable
using UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using UserManagement.Infrastructure.Services;
// using UserManagement.Infrastructure.Helpers;  // Ensure this is included if needed for IHttpContextAccessor

namespace UserManagement.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BSOFT.Api"))
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

            /* var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? ""); */
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                        .Replace("{SERVER}", "192.168.1.126")
                                                        .Replace("{USER_ID}", "developer")
                                                        .Replace("{ENC_PASSWORD}", "Dev@#$456");

            //   var connectionString = ConnectionStringHelper.GetDefaultConnectionString(configuration);

            optionsBuilder.UseSqlServer(connectionString);

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            // Create a dummy or mock IPAddressService implementation
            IIPAddressService ipAddressService = new IPAddressService(httpContextAccessor);
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new ApplicationDbContext(optionsBuilder.Options, ipAddressService, timeZoneService);  // Pass both dependencies
            //return new ApplicationDbContext(optionsBuilder.Options);  // Pass both dependencies
        }
    }
}
