using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using Shared.TestInfrastructure;

namespace SalesManagement.IntegrationTests.Common
{
    // ✅ Single shared DB fixture for ALL tests in this collection
    //    NO parallel execution inside this collection
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "SalesManagement_TestDb";

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
            // 1. Drop & recreate the physical database
            await RecreateDatabaseAsync();

            // 2. Configure mock services
            ConfigureMocks();

            // 3. Create DbContext pointed at the test DB
            await CreateDbContextAsync();

            // 4. Ensure Sales schema exists before EF tries to create tables
            await EnsureSalesSchemaAsync();

            // 5. Create all tables from the EF model (no migrations needed)
            await DbContext.Database.EnsureCreatedAsync();

            // 6. Create cross-module stub tables (Finance.DocumentSequence for ItemPriceMaster)
            await EnsureCrossModuleDependenciesAsync();
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

        private async Task EnsureSalesSchemaAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Sales')
    EXEC('CREATE SCHEMA Sales');
");
        }

        private async Task EnsureCrossModuleDependenciesAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            // ItemPriceMasterCommandRepository.CreateAsync increments Finance.DocumentSequence.DocNo
            await conn.ExecuteAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Finance')
    EXEC('CREATE SCHEMA Finance');

IF NOT EXISTS (SELECT * FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Finance' AND t.name = 'DocumentSequence')
BEGIN
    CREATE TABLE [Finance].[DocumentSequence] (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        TransactionTypeId INT NOT NULL,
        FinancialYearId INT NULL,
        DocNo           INT NOT NULL DEFAULT 0,
        IsActive        BIT NOT NULL DEFAULT 1,
        IsDeleted       BIT NOT NULL DEFAULT 0,
        CreatedBy       INT NOT NULL DEFAULT 0,
        CreatedDate     DATETIMEOFFSET NULL,
        CreatedByName   NVARCHAR(100) NULL,
        CreatedIP       NVARCHAR(50) NULL,
        ModifiedBy      INT NULL,
        ModifiedDate    DATETIMEOFFSET NULL,
        ModifiedByName  NVARCHAR(100) NULL,
        ModifiedIP      NVARCHAR(50) NULL
    );

    -- Seed rows for common transaction types used by ItemPriceMaster
    INSERT INTO [Finance].[DocumentSequence] (TransactionTypeId, FinancialYearId, DocNo)
    VALUES (1, 1, 0), (2, 1, 0), (3, 1, 0), (4, 1, 0), (5, 1, 0);
END
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

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
