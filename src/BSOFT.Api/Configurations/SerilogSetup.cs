using Serilog;
namespace BSOFT.Api.Configurations
{
    public static class SerilogSetup
    {
        public static void ConfigureSerilog(this IHostBuilder hostBuilder, IConfiguration configuration)
        {
            var mongoDbConnection = configuration.GetConnectionString("MongoDbConnectionString");
            var mongoDbDatabase = configuration["MongoDb:DatabaseName"];

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.MongoDB($"{mongoDbConnection}/{mongoDbDatabase}",
                    collectionName: "ApplicationLogs"
                    // restrictedToMinimumLevel: LogEventLevel.Warning
                    )
                .Enrich.FromLogContext()
                .CreateLogger();

            hostBuilder.UseSerilog();
        }
    }
}