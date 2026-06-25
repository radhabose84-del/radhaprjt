using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Logging;
using Shared.TestInfrastructure;

namespace FinanceManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "FinanceManagement_TestDb";

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }

        public string ConnectionString => _testDbConnection;

        public ApplicationDbContext DbContext { get; private set; }

        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        public async Task InitializeAsync()
        {
            await RecreateDatabaseAsync();
            ConfigureMocks();
            await CreateDbContextAsync();
            await EnsureFinanceSchemaAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        private void ConfigureMocks()
        {
            _ip.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            _ip.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");
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
                .UseSqlServer(_testDbConnection, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null))
                .Options;

            DbContext = new ApplicationDbContext(options, _ip.Object, _tz.Object);
            await Task.CompletedTask;
        }

        private async Task EnsureFinanceSchemaAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
    EXEC('CREATE SCHEMA Finance');
");
        }

        public Mock<IIPAddressService> IpMock => _ip;
        public Mock<ITimeZoneService> TzMock => _tz;

        public ApplicationDbContext CreateFreshDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");
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
                .UseSqlServer(_testDbConnection, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null))
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        /// <summary>
        /// Clears all tables in the Finance schema by temporarily disabling FK constraints.
        /// Use this instead of per-test cleanup to avoid FK ordering issues.
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
                WHERE s.name = 'Finance';
                EXEC sp_executesql @disableSql;

                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Finance';
                EXEC sp_executesql @deleteSql;

                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Finance';
                EXEC sp_executesql @enableSql;
            ";

            await conn.ExecuteAsync(sql, commandTimeout: 60);
        }

        // ---- US-GL02-09 audit-trail helpers ----

        public const string AuditTable = "Finance.AccountAuditTrail";
        public const string ImmutabilityTrigger = "trg_AccountAuditTrail_Immutable";

        // Context with the audit-trail interceptor wired in (as production registers it). No retry
        // strategy here — the interceptor's insert path does a nested SaveChanges in SavedChanges.
        public ApplicationDbContext CreateAuditingDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(372);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetUserRole()).Returns("Finance Controller");
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);

            var interceptor = new AccountAuditTrailSaveChangesInterceptor(ipMock.Object, tzMock.Object);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .AddInterceptors((IInterceptor)interceptor)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        public async Task ClearAuditTrailAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync($"DELETE FROM {AuditTable};");
        }

        public async Task<int> CountAuditRowsAsync(string entityName, int entityId)
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(
                $"SELECT COUNT(1) FROM {AuditTable} WHERE EntityName=@e AND EntityId=@id",
                new { e = entityName, id = entityId });
        }

        public async Task InstallImmutabilityTriggerAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync($@"
IF OBJECT_ID('Finance.{ImmutabilityTrigger}') IS NOT NULL DROP TRIGGER Finance.{ImmutabilityTrigger};
EXEC('
CREATE TRIGGER Finance.{ImmutabilityTrigger}
ON {AuditTable}
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    RAISERROR(''AUDIT_TRAIL_IMMUTABLE: Account audit records cannot be modified or deleted.'', 16, 1);
    ROLLBACK TRANSACTION;
END');");
        }

        public async Task DropImmutabilityTriggerAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();
            await conn.ExecuteAsync(
                $"IF OBJECT_ID('Finance.{ImmutabilityTrigger}') IS NOT NULL DROP TRIGGER Finance.{ImmutabilityTrigger};");
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
