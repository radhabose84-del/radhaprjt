using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.Local;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseOrder.Local
{
    /// <summary>
    /// Integration tests for PurchaseOrderQueryRepository (Local PO).
    ///
    /// COMPLEXITY NOTE:
    /// PO query methods involve multi-table SQL JOINs across:
    /// - Purchase.PurchaseOrderHeader, Purchase.PurchaseLocalHeader, Purchase.PurchaseLocalDetail
    /// - Purchase.MiscMaster (status, PO method, approval status)
    /// - Cross-module lookups: Vendor (Party), Item (Inventory), Currency (User)
    ///
    /// Constructor requires: IDbConnection, IIPAddressService, ApplicationDbContext, IMiscMasterQueryRepository
    ///
    /// GetAllAsync is testable with seeded PO data.
    /// Complex queries (GetPOLocalPending, approved indent details) require full PO + indent chain.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class PurchaseOrderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseOrderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseOrderQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = 1, Code = "Approved", Description = "Approved"
                });
            var ctx = _fixture.CreateFreshDbContext();
            return new PurchaseOrderQueryRepository(conn, _fixture.IpMock.Object, ctx, miscMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAsync(1, 10, null, null, null, null, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMyPurchaseOrdersAsync_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetMyPurchaseOrdersAsync(123, 1, 10, null, null, null, null, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        // ---- PO Analysis (GetAnalysisAsync) ----

        [Fact]
        public async Task GetAnalysisAsync_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAnalysisAsync(
                1, 10, null, null, null, null, null, null, null, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        [Fact]
        public async Task GetAnalysisAsync_Should_Apply_Date_And_Amendment_Filters_Without_Error()
        {
            await _fixture.ClearAllTablesAsync();

            var from = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2026, 6, 2, 0, 0, 0, TimeSpan.Zero);

            var result = await CreateRepo().GetAnalysisAsync(
                1, 10, null, null, null, from, to, true, null, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAnalysisAsync_SupplierScope_Should_Return_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAnalysisAsync(
                1, 10, null, null, null, null, null, false, 123, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.Total.Should().Be(0);
        }

        // ---- GetByIdAsync (reverted local-only) ----

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var dto = await CreateRepo().GetByIdAsync(999999, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_LocalOnly_Should_Return_LocalHeader()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedBareHeaderAsync();
            await SeedLocalHeaderAsync(id);

            var dto = await CreateRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Headers.Should().HaveCount(1);
        }

        // ---- GetDetailByIdAsync (all PO types: Local / Import / Contract / Blanket / Service) ----

        [Fact]
        public async Task GetDetailByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var dto = await CreateRepo().GetDetailByIdAsync(999999, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetDetailByIdAsync_Should_Return_Header_And_Run_AllType_Header_Union()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedBareHeaderAsync();

            // Forces the all-type header UNION SQL to compile/execute against the real schema.
            var dto = await CreateRepo().GetDetailByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Headers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDetailByIdAsync_With_LocalHeader_Should_Run_AllType_Detail_Union()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedBareHeaderAsync();
            await SeedLocalHeaderAsync(id);

            // A single local header makes Headers non-empty, which triggers the all-type detail UNION.
            // SQL Server binds every UNION branch at compile time, so this validates the Import / Contract /
            // Blanket / Service detail column references too, even with no rows in those tables.
            var dto = await CreateRepo().GetDetailByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Headers.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetInvoicedTotalAsync_Should_Return_Zero_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var total = await CreateRepo().GetInvoicedTotalAsync(123, CancellationToken.None);

            total.Should().Be(0m);
        }

        [Fact]
        public async Task HasAnyBillEntryAsync_Should_Return_False_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var has = await CreateRepo().HasAnyBillEntryAsync(123, CancellationToken.None);

            has.Should().BeFalse();
        }

        private async Task<int> SeedBareHeaderAsync(int revisionNo = 0)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                ALTER TABLE Purchase.PurchaseOrderHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Purchase.PurchaseOrderHeader
                    (UnitId, PONumber, PODate, POCategoryId, CurrencyId, VendorId,
                     ItemTotal, GSTTotal, StatusId, PurchaseValue, RevisionNo,
                     IsActive, IsDeleted, CreatedBy, CreatedDate)
                VALUES
                    (1, @PONumber, SYSDATETIMEOFFSET(), 1, 1, 1,
                     0, 0, 1, 0, @RevisionNo,
                     1, 0, 1, SYSDATETIMEOFFSET());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                PONumber = "PO-IT-" + Guid.NewGuid().ToString("N").Substring(0, 12),
                RevisionNo = revisionNo
            });
        }

        private async Task SeedLocalHeaderAsync(int purchaseOrderId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                ALTER TABLE Purchase.PurchaseLocalHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Purchase.PurchaseLocalHeader
                    (PurchaseOrderId, IsPartialReceiptAllowed, IsActive, IsDeleted, CreatedBy, CreatedDate)
                VALUES
                    (@PurchaseOrderId, 0, 1, 0, 1, SYSDATETIMEOFFSET());";

            await conn.ExecuteAsync(sql, new { PurchaseOrderId = purchaseOrderId });
        }
    }
}
