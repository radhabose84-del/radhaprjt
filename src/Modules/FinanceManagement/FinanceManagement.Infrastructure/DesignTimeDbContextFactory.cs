using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceManagement.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // EF Core sets CWD to the startup project directory when --startup-project is used.
            // Search upward until we find a directory containing appsettings.json (BSOFT.Api).
            var basePath = FindApiBasePath();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "192.168.1.126")
                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "developer")
                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "Dev@#$456");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(
                optionsBuilder.Options,
                new DesignTimeMockIPAddressService(),
                new TimeZoneService());
        }

        private static string FindApiBasePath()
        {
            // Walk up from CWD until we find a directory that contains appsettings.json
            // (handles both "CWD = startup project" and "CWD = Infrastructure project" cases)
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            // Fallback: assume CWD is Infrastructure dir, go up 3 levels to src/ then into BSOFT.Api
            return Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "BSOFT.Api");
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
            public int? GetEmpId() => null;
        }
    }
}
