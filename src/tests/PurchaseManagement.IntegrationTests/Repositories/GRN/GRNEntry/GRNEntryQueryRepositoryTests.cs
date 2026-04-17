using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.GRN.GRNEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.GRN.GRNEntry
{
    /// <summary>
    /// Integration tests for GRNEntryQueryRepository.
    ///
    /// COMPLEXITY NOTE:
    /// GRN query methods rely on deeply joined SQL across:
    /// - Purchase.GrnHeader, Purchase.GrnDetail, Purchase.GrnPutAwayRule
    /// - Purchase.PurchaseOrderHeader, Purchase.PurchaseLocalHeader/Detail
    /// - Purchase.GateEntryHeader, Purchase.StockLedger
    /// - Cross-module: Items, UOMs, Warehouses, Vendors
    ///
    /// The GetDocumentDirectoryAsync method is testable (reads from MiscTypeMaster).
    /// Complex query methods (GetPendingGateEntriesForGrnAsync, GetPoPendingForGrn, etc.)
    /// require full PO -> GateEntry -> GRN chain seeding and are covered by unit tests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class GRNEntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GRNEntryQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GRNEntryQueryRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new GRNEntryQueryRepository(conn, _fixture.IpMock.Object);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var repo = CreateRepo();
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Description_When_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // Seed MiscTypeMaster with GrnReceivedImage code
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "GrnReceivedImage",
                Description = "/uploads/grn/",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(mt);
            await ctx.SaveChangesAsync();

            var result = await CreateRepo().GetDocumentDirectoryAsync();

            result.Should().Be("/uploads/grn/");
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetDocumentDirectoryAsync();

            result.Should().BeNull();
        }
    }
}
