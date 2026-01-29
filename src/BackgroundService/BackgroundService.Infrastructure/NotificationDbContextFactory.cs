using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BackgroundService.Infrastructure
{
    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
             var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";                        
            // Build configuration 
             IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BackgroundService.Api"))
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

          var connectionString = configuration.GetConnectionString("NotificationConnection")
                                                .Replace("{SERVER}","192.168.1.126")
                                                .Replace("{USER_ID}","Developer")
                                                .Replace("{ENC_PASSWORD}", "Dev@#$456");


            optionsBuilder.UseSqlServer(connectionString);
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            IIPAddressService ipAddressService = new IPAddressService(httpContextAccessor);
            ITimeZoneService timeZoneService = new TimeZoneService();           

            return new NotificationDbContext(optionsBuilder.Options, ipAddressService,timeZoneService); 
        }
    }
}