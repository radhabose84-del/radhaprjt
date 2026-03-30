using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "WarehouseManagement_TestDb";

        private const string MasterConnection =
            "Server=192.168.1.126;Database=master;User Id=developer;Password=Dev@#$456;Encrypt=False;TrustServerCertificate=True;";

        private const string TestDbConnection =
            "Server=192.168.1.126;Database=WarehouseManagement_TestDb;User Id=developer;Password=Dev@#$456;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        public string ConnectionString => TestDbConnection;

        public ApplicationDbContext? DbContext { get; private set; }

        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            await CreateDbContextAsync();
            await EnsureWarehouseSchemaAsync();
            await DbContext!.Database.EnsureCreatedAsync();
        }

        private void ConfigureMocks()
        {
            _ip.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            _ip.Setup(x => x.GetUserId()).Returns(1);
            _ip.Setup(x => x.GetUserName()).Returns("test-user");
            _ip.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            _ip.Setup(x => x.GetUnitId()).Returns(1);
            _ip.Setup(x => x.GetCompanyId()).Returns(1);
            _ip.Setup(x => x.GetEntityId()).Returns(1);

            _tz.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            _tz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);
        }

        private async Task RecreateDatabaseAsync()
        {
            await using var cnn = new SqlConnection(MasterConnection);
            await cnn.OpenAsync();

            var sql = $@"
IF EXISTS (SELECT * FROM sys.databases WHERE name = '{DbName}')
BEGIN
    ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{DbName}];
END;

CREATE DATABASE [{DbName}];
";
            await cnn.ExecuteAsync(sql);
        }

        private async Task CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(TestDbConnection)
                .Options;

            DbContext = new ApplicationDbContext(options, _ip.Object, _tz.Object);
            await Task.CompletedTask;
        }

        private async Task EnsureWarehouseSchemaAsync()
        {
            await DbContext!.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Warehouse')
    EXEC('CREATE SCHEMA Warehouse');
");
        }

        public Mock<IIPAddressService> IpMock => _ip;
        public Mock<ITimeZoneService> TzMock => _tz;

        public ApplicationDbContext CreateFreshDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(TestDbConnection)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
