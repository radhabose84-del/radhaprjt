using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Infrastructure.Data;
using Shared.TestInfrastructure;

namespace BudgetManagement.IntegrationTests.Common
{
    [CollectionDefinition("DatabaseCollection", DisableParallelization = true)]
    public class DatabaseCollection : ICollectionFixture<DbFixture> { }

    public class DbFixture : IAsyncLifetime
    {
        private const string DbName = "BudgetManagement_TestDb";

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
            await EnsureBudgetSchemaAsync();
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

        private async Task EnsureBudgetSchemaAsync()
        {
            await DbContext.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Budget')
    EXEC('CREATE SCHEMA Budget');
");
        }

        /// <summary>Seeds MiscTypeMaster(1), MiscMaster(1), BudgetGroup(1) as FK prerequisites.
        /// Call from ClearTableAsync in test classes that depend on these records.</summary>
        public async Task SeedPrerequisiteDataAsync()
        {
            await using var conn = new SqlConnection(_testDbConnection);
            await conn.OpenAsync();

            // 1. MiscTypeMaster — prerequisite for MiscMaster
            await conn.ExecuteAsync(@"
SET IDENTITY_INSERT Budget.MiscTypeMaster ON;
IF NOT EXISTS (SELECT 1 FROM Budget.MiscTypeMaster WHERE Id = 1)
    INSERT INTO Budget.MiscTypeMaster
        (Id, MiscTypeCode, Description, IsActive, IsDeleted,
         CreatedBy, CreatedByName, CreatedIP)
    VALUES
        (1, 'TEST', 'Test Type', 1, 0,
         1, 'test-user', '127.0.0.1');
SET IDENTITY_INSERT Budget.MiscTypeMaster OFF;
");

            // 2. MiscMaster — FK target for RequestTypeId / RequestById / RequestMonthId / AllocationTypeId
            //    Seed Id=1,2,3 to support tests that use requestById values of 1, 2 or 3
            await conn.ExecuteAsync(@"
SET IDENTITY_INSERT Budget.MiscMaster ON;
IF NOT EXISTS (SELECT 1 FROM Budget.MiscMaster WHERE Id = 1)
    INSERT INTO Budget.MiscMaster
        (Id, MiscTypeId, Code, description, sortOrder, IsActive, IsDeleted,
         CreatedBy, CreatedByName, CreatedIP)
    VALUES (1, 1, 'TEST1', 'Test Misc 1', 0, 1, 0, 1, 'test-user', '127.0.0.1');
IF NOT EXISTS (SELECT 1 FROM Budget.MiscMaster WHERE Id = 2)
    INSERT INTO Budget.MiscMaster
        (Id, MiscTypeId, Code, description, sortOrder, IsActive, IsDeleted,
         CreatedBy, CreatedByName, CreatedIP)
    VALUES (2, 1, 'TEST2', 'Test Misc 2', 0, 1, 0, 1, 'test-user', '127.0.0.1');
IF NOT EXISTS (SELECT 1 FROM Budget.MiscMaster WHERE Id = 3)
    INSERT INTO Budget.MiscMaster
        (Id, MiscTypeId, Code, description, sortOrder, IsActive, IsDeleted,
         CreatedBy, CreatedByName, CreatedIP)
    VALUES (3, 1, 'TEST3', 'Test Misc 3', 0, 1, 0, 1, 'test-user', '127.0.0.1');
SET IDENTITY_INSERT Budget.MiscMaster OFF;
");

            // 3. BudgetGroup — FK target for BudgetGroupId
            await conn.ExecuteAsync(@"
SET IDENTITY_INSERT Budget.BudgetGroup ON;
IF NOT EXISTS (SELECT 1 FROM Budget.BudgetGroup WHERE Id = 1)
    INSERT INTO Budget.BudgetGroup
        (Id, Name, UnitId, DepartmentId, CostCenterId, CurrencyId,
         IsParent, CarryForward, IsActive, IsDeleted,
         CreatedBy, CreatedByName, CreatedIP, CreatedDate)
    VALUES
        (1, 'Test Group', 1, 1, 1, 1,
         0, 0, 1, 0,
         1, 'test-user', '127.0.0.1', SYSDATETIMEOFFSET());
SET IDENTITY_INSERT Budget.BudgetGroup OFF;
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
