#nullable disable
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using FAM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using FAM.Infrastructure.Services;

namespace FAM.Infrastructure
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

          
            //var connectionString = ConnectionStringHelper.GetDefaultConnectionString(configuration);
            
          var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}","192.168.1.126")
                                                .Replace("{USER_ID}","Developer")
                                                .Replace("{ENC_PASSWORD}", "Dev@#$456");

            optionsBuilder.UseSqlServer(connectionString);

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            IIPAddressService ipAddressService = new IPAddressService(httpContextAccessor);
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new ApplicationDbContext(optionsBuilder.Options, ipAddressService, timeZoneService);
        }
    }
}
