using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Quotation.QuotationEntry
{
    /// <summary>
    /// Integration tests for QuotationCommandRepository.
    ///
    /// COMPLEXITY NOTE:
    /// Quotation entries are linked to RFQ (Request for Quotation):
    /// - QuotationHeader requires: RfqId (FK to RfqMaster), SupplierId (cross-module)
    /// - QuotationLine requires: QuotationHeaderId, ItemId (cross-module), UomId (cross-module)
    /// - ExistsForSupplierRfqAsync checks composite uniqueness (SupplierId + RfqId)
    ///
    /// Constructor uses primary constructor: (ApplicationDbContext db, IDbConnection dbConnection)
    ///
    /// Basic existence checks are testable with minimal seeding.
    /// Full CRUD requires seeding RfqMaster first.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class QuotationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QuotationCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QuotationCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new QuotationCommandRepository(ctx, conn);
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);
            repo.Should().NotBeNull();
        }

        [Fact]
        public async Task ExistsForSupplierRfqAsync_Should_Return_False_When_Empty()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsForSupplierRfqAsync(1, 1, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsForSupplierRfqOtherAsync_Should_Return_False_When_Empty()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsForSupplierRfqOtherAsync(1, 1, 1, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetWithLinesAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetWithLinesAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
