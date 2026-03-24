using Contracts.Interfaces;
using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Infrastructure.Data;
using GateEntryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GateEntryManagement.Infrastructure
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

          var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                                                .Replace("{SERVER}","192.168.1.126")
                                                .Replace("{USER_ID}","Developer")
                                                .Replace("{ENC_PASSWORD}", "Dev@#$456");


            optionsBuilder.UseSqlServer(connectionString);

            IIPAddressService ipAddressService = new DesignTimeMockIPAddressService();
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new ApplicationDbContext(optionsBuilder.Options, ipAddressService,timeZoneService);  // Pass both dependencies
            //return new ApplicationDbContext(optionsBuilder.Options);  // Pass both dependencies
        }

        private sealed class DesignTimeMockIPAddressService : IIPAddressService
        {
            public string GetSystemIPAddress() => "127.0.0.1";
            public string GetUserIPAddress() => "127.0.0.1";
            public string GetUserAgent() => string.Empty;
            public string GetCurrentUserId() => "0";
            public int GetUserId() => 0;
            public string GetUserName() => "design-time";
            public string GetUserOS() => string.Empty;
            public string GetUserBrowserDetails(string userAgent) => string.Empty;
            public int? GetCompanyId() => null;
            public string GetGroupCode() => string.Empty;
            public int GetEntityId() => 0;
            public int? GetUnitId() => null;
            public string GetOldUnitId() => string.Empty;
            public int? GetPartyId() => null;
        }
    }
}
