using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.Data.SqlClient;
using Dapper;
using Shared.TestInfrastructure;

namespace BackgroundService.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "BackgroundService_TestDb";

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public string ConnectionString => _testDbConnection;

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }
        public NotificationDbContext DbContext { get; private set; }

        private Mock<IIPAddressService> _mockIpService;
        private Mock<ITimeZoneService> _mockTimeZoneService;

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            DbContext = CreateFreshDbContext();
            await EnsureSchemasAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        public NotificationDbContext CreateFreshDbContext()
        {
            var options = new DbContextOptionsBuilder<NotificationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;
            return new NotificationDbContext(options, _mockIpService.Object, _mockTimeZoneService.Object);
        }

        private void ConfigureMocks()
        {
            _mockIpService = new Mock<IIPAddressService>();
            _mockIpService.Setup(s => s.GetUserIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);
            _mockIpService.Setup(s => s.GetCurrentUserId()).Returns("1");
            _mockIpService.Setup(s => s.GetUserAgent()).Returns("test-agent");
            _mockIpService.Setup(s => s.GetUserOS()).Returns("Windows");
            _mockIpService.Setup(s => s.GetUserBrowserDetails(It.IsAny<string>())).Returns("Test Browser");
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("SUPER_ADMIN");
            _mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            _mockIpService.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIpService.Setup(s => s.GetEntityId()).Returns(1);
            _mockIpService.Setup(s => s.GetOldUnitId()).Returns("1");
            _mockIpService.Setup(s => s.GetPartyId()).Returns(1);

            _mockTimeZoneService = new Mock<ITimeZoneService>();
            _mockTimeZoneService.Setup(s => s.GetSystemTimeZone()).Returns("UTC");
            _mockTimeZoneService.Setup(s => s.GetCurrentTime(It.IsAny<string>()))
                .Returns(DateTimeOffset.UtcNow);
            _mockTimeZoneService.Setup(s => s.ConvertUtcToTimeZone(It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
                .Returns<DateTimeOffset, string>((dt, _) => dt);
        }

        private async Task RecreateDatabaseAsync()
        {
            await using var conn = new SqlConnection(_masterConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF EXISTS (SELECT * FROM sys.databases WHERE name = N'BackgroundService_TestDb')
                BEGIN
                    ALTER DATABASE [BackgroundService_TestDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [BackgroundService_TestDb];
                END
                CREATE DATABASE [BackgroundService_TestDb];
            ");
        }

        private async Task EnsureSchemasAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                "IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AppData') EXEC('CREATE SCHEMA [AppData]')");
            await conn.ExecuteAsync(
                "IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AppNotification') EXEC('CREATE SCHEMA [AppNotification]')");
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
