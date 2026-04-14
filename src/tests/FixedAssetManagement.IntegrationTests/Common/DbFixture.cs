using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Application.Common.Interfaces;
using FAM.Infrastructure.Data;
using Shared.TestInfrastructure;

namespace FixedAssetManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "FixedAssetManagement_TestDb";

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public string ConnectionString => _testDbConnection;

        public ApplicationDbContext DbContext { get; private set; }

        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            await CreateDbContextAsync();
            await EnsureFixedAssetSchemaAsync();
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
            await cnn.ExecuteAsync(sql);
        }

        private async Task CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;

            DbContext = new ApplicationDbContext(options, _ip.Object, _tz.Object);
            await Task.CompletedTask;
        }

        private async Task EnsureFixedAssetSchemaAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'FixedAsset')
    EXEC('CREATE SCHEMA FixedAsset');
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
        /// Clears all tables in the FixedAsset schema by temporarily disabling FK constraints.
        /// Use this instead of per-test ClearTablesAsync to avoid FK ordering issues.
        /// </summary>
        public async Task ClearAllTablesAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            // Disable all FK constraints in FixedAsset schema
            await conn.ExecuteAsync(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' NOCHECK CONSTRAINT ALL;'
                FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'FixedAsset';
                EXEC sp_executesql @sql;");

            // Delete from all tables in FixedAsset schema
            await conn.ExecuteAsync(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';'
                FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'FixedAsset';
                EXEC sp_executesql @sql;");

            // Re-enable all FK constraints
            await conn.ExecuteAsync(@"
                DECLARE @sql NVARCHAR(MAX) = N'';
                SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' WITH CHECK CHECK CONSTRAINT ALL;'
                FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'FixedAsset';
                EXEC sp_executesql @sql;");
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
