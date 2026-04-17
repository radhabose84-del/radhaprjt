using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.TestInfrastructure;
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

        private readonly string _masterConnection;
        private readonly string _testDbConnection;

        public string ConnectionString => _testDbConnection;

        public DbFixture()
        {
            (_masterConnection, _testDbConnection) = TestConnectionFactory.Build(DbName);
        }

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
            _ip.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            _ip.Setup(x => x.GetUnitId()).Returns(1);
            _ip.Setup(x => x.GetCompanyId()).Returns(1);
            _ip.Setup(x => x.GetEntityId()).Returns(1);

            _tz.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
        }

        private async Task RecreateDatabaseAsync()
        {
            await using var cnn = new Microsoft.Data.SqlClient.SqlConnection(_masterConnection);
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
                .UseSqlServer(_testDbConnection)
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

        /// <summary>
        /// Clears all tables in the AppSecurity, AppData, and AppMaster schemas
        /// by temporarily disabling FK constraints.
        /// </summary>
        public async Task ClearAllTablesAsync()
        {
            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            const string sql = @"
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name IN ('AppSecurity','AppData','AppMaster');
                EXEC sp_executesql @disableSql;

                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name IN ('AppSecurity','AppData','AppMaster');
                EXEC sp_executesql @deleteSql;

                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' WITH CHECK CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name IN ('AppSecurity','AppData','AppMaster');
                EXEC sp_executesql @enableSql;
            ";

            await conn.ExecuteAsync(sql, commandTimeout: 60);
        }

        public ApplicationDbContext CreateFreshDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_testDbConnection)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
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
