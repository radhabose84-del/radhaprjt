using Contracts.Interfaces;
using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Shared.TestInfrastructure;

namespace GateEntryManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "GateEntry_TestDb";

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public string ConnectionString => _testDbConnection;

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }
        public ApplicationDbContext DbContext { get; private set; }

        private Mock<IIPAddressService> _mockIpService;
        private Mock<ITimeZoneService> _mockTimeZoneService;

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            await CreateDbContextAsync();
            await EnsureSchemaAsync();
            await DbContext.Database.EnsureCreatedAsync();
            await CreateCrossModuleStubsAsync();
        }

        public ApplicationDbContext CreateFreshDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;
            return new ApplicationDbContext(options, _mockIpService.Object, _mockTimeZoneService.Object);
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

        private async Task CreateDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;
            DbContext = new ApplicationDbContext(options, _mockIpService.Object, _mockTimeZoneService.Object);
        }

        private async Task RecreateDatabaseAsync()
        {
            await using var conn = new SqlConnection(_masterConnection);
            await conn.OpenAsync();

            // Kill existing connections before dropping
            await conn.ExecuteAsync(@"
                IF EXISTS (SELECT * FROM sys.databases WHERE name = N'GateEntry_TestDb')
                BEGIN
                    ALTER DATABASE [GateEntry_TestDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [GateEntry_TestDb];
                END
                CREATE DATABASE [GateEntry_TestDb];
            ");
        }

        private async Task EnsureSchemaAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                "IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Gate') EXEC('CREATE SCHEMA [Gate]')");
        }

        /// <summary>
        /// Creates stub tables for cross-module JOINs used in Dapper queries.
        /// Called AFTER EnsureCreatedAsync so EF Core tables exist first.
        /// </summary>
        private async Task CreateCrossModuleStubsAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                "IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Finance') EXEC('CREATE SCHEMA [Finance]')");
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Finance' AND TABLE_NAME = 'TransactionTypeMaster')
                CREATE TABLE Finance.TransactionTypeMaster (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    ShortName VARCHAR(50),
                    TypeName VARCHAR(100),
                    IsDeleted BIT NOT NULL DEFAULT 0
                )");
        }

        /// <summary>
        /// Clears all tables in the Gate schema by temporarily disabling FK constraints.
        /// </summary>
        public async Task ClearAllTablesAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            const string sql = @"
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Gate';
                EXEC sp_executesql @disableSql;

                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Gate';
                EXEC sp_executesql @deleteSql;

                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Gate';
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
