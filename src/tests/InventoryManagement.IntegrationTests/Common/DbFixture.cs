using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Shared.TestInfrastructure;

namespace InventoryManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "InventoryManagement_TestDb";

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public string ConnectionString => _testDbConnection;

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }

        public ApplicationDbContext DbContext { get; private set; }

        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            await CreateDbContextAsync();
            await EnsureInventorySchemaAsync();
            await DbContext.Database.EnsureCreatedAsync();
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
            await using var cnn = new SqlConnection(_masterConnection);
            await cnn.OpenAsync();

            var sql = $@"
IF EXISTS (SELECT * FROM sys.databases WHERE name = '{DbName}')
BEGIN
    ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{DbName}];
END;

CREATE DATABASE [{DbName}];
";
            await cnn.ExecuteAsync(sql, commandTimeout: 120);
        }

        private async Task CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;

            DbContext = new ApplicationDbContext(options, _ip.Object, _tz.Object);
            await Task.CompletedTask;
        }

        private async Task EnsureInventorySchemaAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Inventory')
    EXEC('CREATE SCHEMA Inventory');
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
                .UseSqlServer(_testDbConnection)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        /// <summary>
        /// Deletes all rows from Inventory tables in FK-safe order (children first).
        /// Use this instead of per-test ClearAsync methods to avoid FK constraint violations.
        /// </summary>
        public async Task ClearAllTablesAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            const string sql = @"
                -- Disable all FK constraints in Inventory schema
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Inventory';
                EXEC sp_executesql @disableSql;

                -- Delete all data from Inventory schema tables
                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Inventory';
                EXEC sp_executesql @deleteSql;

                -- Re-enable all FK constraints
                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Inventory';
                EXEC sp_executesql @enableSql;
            ";

            await conn.ExecuteAsync(sql, commandTimeout: 60);
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
