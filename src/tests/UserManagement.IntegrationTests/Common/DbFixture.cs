using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using Xunit;

namespace UserManagement.IntegrationTests.Common
{
    // ✅ Single shared DB fixture for ALL tests in this collection
    //    and NO parallel execution inside this collection
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "UserManagement_TestDb";

        private const string MasterConnection =
            "Server=192.168.1.126;Database=master;User Id=developer;Password=Dev@#$456;Encrypt=False;TrustServerCertificate=True;";

        private const string TestDbConnection =
            "Server=192.168.1.126;Database=UserManagement_TestDb;User Id=developer;Password=Dev@#$456;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        public string ConnectionString => TestDbConnection;

        public ApplicationDbContext DbContext { get; private set; } = default!;

        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        public async Task InitializeAsync()
        {
            // 1. Drop & recreate the physical database
            await RecreateDatabaseAsync();

            // 2. Configure injected services
            ConfigureMocks();

            // 3. Create DbContext
            await CreateDbContextAsync();

            // 4. Ensure schemas exist (so EF can create tables in those schemas)
            await EnsureSchemaExistsAsync();

            // 5. Create schema from the current EF model (NO migrations)
            await DbContext.Database.EnsureCreatedAsync();
        }

        private void ConfigureMocks()
        {
            _ip.Setup(x => x.GetGroupcode()).Returns("SUPER_ADMIN");
            _ip.Setup(x => x.GetUnitId()).Returns(1);
            _ip.Setup(x => x.GetCompanyId()).Returns(1);
            _ip.Setup(x => x.GetEntityId()).Returns(1);

            _tz.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
        }

        private async Task RecreateDatabaseAsync()
        {
            await using var cnn = new Microsoft.Data.SqlClient.SqlConnection(MasterConnection);
            await cnn.OpenAsync();

            var killSql = $@"
IF EXISTS (SELECT * FROM sys.databases WHERE name = '{DbName}')
BEGIN
    ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{DbName}];
END;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{DbName}')
BEGIN
    CREATE DATABASE [{DbName}];
END;
";
            await cnn.ExecuteAsync(killSql);
        }

        private async Task CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(TestDbConnection)
                .Options;

            DbContext = new ApplicationDbContext(options, _ip.Object, _tz.Object);

            await Task.CompletedTask;
        }

        private async Task EnsureSchemaExistsAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'AppSecurity')
    EXEC('CREATE SCHEMA AppSecurity');

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'AppData')
    EXEC('CREATE SCHEMA AppData');

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'AppMaster')
    EXEC('CREATE SCHEMA AppMaster');
");
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
            {
                await DbContext.DisposeAsync();
            }
        }
    }
}
