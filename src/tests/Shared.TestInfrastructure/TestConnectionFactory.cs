using Microsoft.Extensions.Configuration;

namespace Shared.TestInfrastructure
{
    /// <summary>
    /// Builds SQL Server connection strings for integration tests.
    ///
    /// Resolution order (last wins):
    ///   1. appsettings.Test.json              — Production defaults (192.168.1.135)
    ///   2. appsettings.Test.{ENV}.json        — Environment-specific (Development, QA)
    ///   3. Environment variables (TEST_DB_)   — CI/CD pipeline overrides
    ///
    /// Environment selection:
    ///   Set TEST_ENVIRONMENT=Development | QA | Production (default: Production)
    ///
    /// CI/CD override examples:
    ///   TEST_DB_TestDatabase__Server=ci-sql-server
    ///   TEST_DB_TestDatabase__UserId=ci_user
    ///   TEST_DB_TestDatabase__Password=CiPass123
    ///
    /// Usage in DbFixture:
    ///   var (masterConn, testDbConn) = TestConnectionFactory.Build("FixedAssetManagement_TestDb");
    /// </summary>
    public static class TestConnectionFactory
    {
        public static (string MasterConnection, string TestDbConnection) Build(string testDatabaseName)
        {
            var env = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Development";

            var basePath = FindSettingsDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Test.json", optional: false)
                .AddJsonFile($"appsettings.Test.{env}.json", optional: true)
                .AddEnvironmentVariables(prefix: "TEST_DB_")
                .Build();

            var server = config["TestDatabase:Server"];
            var userId = config["TestDatabase:UserId"];
            var password = config["TestDatabase:Password"];
            var encrypt = config["TestDatabase:Encrypt"] ?? "False";
            var trustCert = config["TestDatabase:TrustServerCertificate"] ?? "True";

            var baseConn = $"Server={server};User Id={userId};Password={password};" +
                           $"Encrypt={encrypt};TrustServerCertificate={trustCert};";

            var masterConn = baseConn + "Database=master;";
            var testDbConn = baseConn + $"Database={testDatabaseName};MultipleActiveResultSets=true;";

            return (masterConn, testDbConn);
        }

        /// <summary>
        /// Walks up from the executing assembly's directory to find appsettings.Test.json.
        /// Handles both "dotnet test" (bin/Debug/net8.0/) and IDE test runners.
        /// </summary>
        private static string FindSettingsDirectory()
        {
            var currentDir = AppContext.BaseDirectory;
            if (File.Exists(Path.Combine(currentDir, "appsettings.Test.json")))
                return currentDir;

            var dir = new DirectoryInfo(currentDir);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "appsettings.Test.json");
                if (File.Exists(candidate))
                    return dir.FullName;
                dir = dir.Parent;
            }

            return currentDir;
        }
    }
}
