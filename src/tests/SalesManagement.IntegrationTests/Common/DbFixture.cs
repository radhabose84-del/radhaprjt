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

        /// <summary>
        /// Clears every Sales table that holds a FK to MiscMaster (directly or via a chain),
        /// in FK-safe order, then deletes MiscMaster rows. Use from any test that needs a
        /// clean MiscMaster state — e.g. MiscMaster / MiscTypeMaster tests.
        /// </summary>
        public async Task ClearAllMiscMasterDependentsAsync()
        {
            await using var cnn = new SqlConnection(_testDbConnection);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync(@"
                DELETE FROM Sales.AgentCommissionSlab;
                DELETE FROM Sales.AgentCommissionPaymentTerm;
                DELETE FROM Sales.AgentCommissionSalesGroup;
                DELETE FROM Sales.AgentCommissionConfig;
                DELETE FROM Sales.CommissionSplitDetail;
                DELETE FROM Sales.CommissionSplit;

                -- Transactional aggregates referencing MiscMaster (and their child rows)
                DELETE FROM Sales.ComplaintQCReviewAssignment;
                DELETE FROM Sales.SalesReturnDetail;
                DELETE FROM Sales.SalesReturnHeader;
                DELETE FROM Sales.ComplaintResolution;
                DELETE FROM Sales.ComplaintHeader;
                DELETE FROM Sales.ProformaInvoice;
                DELETE FROM Sales.InvoiceDetail;
                DELETE FROM Sales.InvoiceHeader;
                DELETE FROM Sales.DispatchAdviceDetail;
                DELETE FROM Sales.DispatchAdviceHeader;
                DELETE FROM Sales.InvoiceHeader;
                DELETE FROM Sales.SalesOrderAmendmentDetail;
                DELETE FROM Sales.SalesOrderAmendmentHeader;
                DELETE FROM Sales.SalesOrderDetail;
                DELETE FROM Sales.SalesOrderHeader;
                DELETE FROM Sales.SalesQuotationDetail;
                DELETE FROM Sales.SalesQuotationHeader;

                -- MarketingOfficer dependents populated by other test classes
                DELETE FROM Sales.OfficerAgent;
                DELETE FROM Sales.CustomerVisitProduct;
                DELETE FROM Sales.CustomerVisit;
                DELETE FROM Sales.DispatchAddressMapping;
                DELETE FROM Sales.ItemPriceMaster;
                DELETE FROM Sales.AgentCustomerMapping;
                DELETE FROM Sales.SalesLead;
                DELETE FROM Sales.SalesContact;

                -- STO chain (references MovementTypeConfig → MiscMaster)
                DELETE FROM Sales.StoReceiptDetail;
                DELETE FROM Sales.StoReceiptHeader;
                DELETE FROM Sales.DeliveryChallanDetail;
                DELETE FROM Sales.DeliveryChallanHeader;
                DELETE FROM Sales.StoDetail;
                DELETE FROM Sales.StoHeader;
                DELETE FROM Sales.StoTypeMaster;
                DELETE FROM Sales.MovementTypeConfig;

                DELETE FROM Sales.MiscMaster;
            ");
        }

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
            // Clear all pooled connections first — stale connections to the old (dropped) DB
            // cause "Login failed for user" errors when the pool tries to reuse them.
            SqlConnection.ClearAllPools();

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
            await cnn.ExecuteAsync(sql, commandTimeout: 60);

            // Clear pools again after DB recreation to ensure no stale connections remain.
            SqlConnection.ClearAllPools();
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

        /// <summary>
        /// Clears only the specified tables with FK constraints temporarily disabled.
        /// Use this for tests that seed prerequisite data and only need to clear the entity under test.
        /// </summary>
        public async Task ClearTablesAsync(params string[] tableNames)
        {
            var deleteSql = string.Join("\n", tableNames.Select(t => $"DELETE FROM {t};"));
            var sql = $@"
                DECLARE @d NVARCHAR(MAX)=N'',@e NVARCHAR(MAX)=N'';
                SELECT @d+='ALTER TABLE '+QUOTENAME(s.name)+'.'+QUOTENAME(t.name)+' NOCHECK CONSTRAINT ALL;'
                FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE s.name='Sales';
                EXEC sp_executesql @d;

                {deleteSql}

                SET @e=N'';
                SELECT @e+='ALTER TABLE '+QUOTENAME(s.name)+'.'+QUOTENAME(t.name)+' CHECK CONSTRAINT ALL;'
                FROM sys.tables t JOIN sys.schemas s ON t.schema_id=s.schema_id WHERE s.name='Sales';
                EXEC sp_executesql @e;
            ";

            await ExecuteWithDeadlockRetryAsync(sql);
        }

        // SQL Server error 1205 = deadlock victim. The `ALTER TABLE ... NOCHECK CONSTRAINT ALL`
        // sweep over every table in the Sales schema takes heavy schema locks and can lose a
        // deadlock race with any other concurrent session. The error message itself says
        // "Rerun the transaction" — this helper does that with short backoff.
        private async Task ExecuteWithDeadlockRetryAsync(string sql, int maxAttempts = 5)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    await using var conn = new SqlConnection(_testDbConnection);
                    await conn.OpenAsync();
                    await conn.ExecuteAsync(sql, commandTimeout: 60);
                    return;
                }
                catch (SqlException ex) when (ex.Number == 1205 && attempt < maxAttempts)
                {
                    await Task.Delay(100 * attempt);
                }
            }
        }

        /// <summary>
        /// Shared prerequisite table names that are excluded from ClearAllTablesAsync()
        /// to avoid destroying reference data seeded by EnsurePrerequisitesAsync() in other test classes.
        /// Tests that specifically test these entities should use ClearAllTablesIncludingPrerequisitesAsync().
        /// </summary>
        private static readonly string[] PrerequisiteTableNames =
        {
            "MiscTypeMaster", "MiscMaster", "SalesOrganisation", "SalesChannel", "BusinessUnit",
            "SalesOffice", "SalesGroup", "SalesSegment"
        };

        /// <summary>
        /// Clears all tables in the Sales schema EXCEPT shared prerequisite tables
        /// (MiscTypeMaster, MiscMaster, SalesOrganisation, SalesChannel, BusinessUnit).
        /// This prevents wiping reference data that other test classes seed via EnsurePrerequisitesAsync().
        /// Use ClearAllTablesIncludingPrerequisitesAsync() for tests that specifically test those entities.
        /// </summary>
        public async Task ClearAllTablesAsync()
        {
            var exclusionList = string.Join(",", PrerequisiteTableNames.Select(n => $"N'{n}'"));

            var sql = $@"
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales';
                EXEC sp_executesql @disableSql;

                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales'
                  AND t.name NOT IN ({exclusionList});
                EXEC sp_executesql @deleteSql;

                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales';
                EXEC sp_executesql @enableSql;
            ";

            await ExecuteWithDeadlockRetryAsync(sql);
        }

        /// <summary>
        /// Clears ALL tables in the Sales schema including shared prerequisite tables.
        /// Use this ONLY in test classes that specifically test MiscTypeMaster, MiscMaster,
        /// SalesOrganisation, SalesChannel, or BusinessUnit entities.
        /// </summary>
        public async Task ClearAllTablesIncludingPrerequisitesAsync()
        {
            const string sql = @"
                DECLARE @disableSql NVARCHAR(MAX) = N'';
                SELECT @disableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' NOCHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales';
                EXEC sp_executesql @disableSql;

                DECLARE @deleteSql NVARCHAR(MAX) = N'';
                SELECT @deleteSql += 'DELETE FROM ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales';
                EXEC sp_executesql @deleteSql;

                DECLARE @enableSql NVARCHAR(MAX) = N'';
                SELECT @enableSql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
                    + ' CHECK CONSTRAINT ALL;' + CHAR(13)
                FROM sys.tables t
                JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = 'Sales';
                EXEC sp_executesql @enableSql;
            ";

            await ExecuteWithDeadlockRetryAsync(sql);
        }

        public async Task DisposeAsync()
        {
            if (DbContext != null)
                await DbContext.DisposeAsync();
        }
    }
}
