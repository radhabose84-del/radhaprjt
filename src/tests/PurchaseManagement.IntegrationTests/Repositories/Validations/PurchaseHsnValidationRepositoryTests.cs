using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseHsnValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseHsnValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PurchaseHsnValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PurchaseHsnValidationRepository(conn);
        }

        // ── Seed helpers ─────────────────────────────────────────────────────

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync();
            if (existing != null) return existing.Id;

            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "RfqStatus",
                Description = "RFQ Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "Draft",
                Description = "Draft",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedRfqMasterAsync()
        {
            var miscId = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var rfq = new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqMaster
            {
                UnitId = 1,
                RfqCode = "RFQ-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                RfqStatusId = miscId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Rfqs.Add(rfq);
            await ctx.SaveChangesAsync();
            return rfq.Id;
        }

        private async Task SeedRfqItemAsync(int rfqId, int hsnId, int itemId = 1, int uomId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.RfqItems.Add(new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqItem
            {
                RfqId = rfqId,
                ItemId = itemId,
                HsnId = hsnId,
                Quantity = 10m,
                UomId = uomId
            });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> SeedQuotationHeaderAsync(int rfqId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var header = new PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader
            {
                UnitId = 1,
                RfqId = rfqId,
                SupplierId = 1,
                QuotationNumber = "Q-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                ValidTill = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.QuotationHeaders.Add(header);
            await ctx.SaveChangesAsync();
            return header.Id;
        }

        private async Task SeedQuotationDetailAsync(
            int headerId, int hsnId,
            int itemId = 1, int uomId = 1, int currencyId = 1,
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.QuotationDetails.Add(new PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationDetail
            {
                QuotationHeaderId = headerId,
                ItemId = itemId,
                HsnId = hsnId,
                Quantity = 10m,
                UomId = uomId,
                CurrencyId = currencyId,
                Rate = 100m,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        // ── HasLinkedHsnAsync ────────────────────────────────────────────────

        [Fact]
        public async Task HasLinkedHsnAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedHsnAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedHsnAsync_Returns_True_When_Referenced_By_RfqItem()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, hsnId: 42);

            var result = await CreateRepo().HasLinkedHsnAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedHsnAsync_Returns_True_When_Referenced_By_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, hsnId: 77);

            var result = await CreateRepo().HasLinkedHsnAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedHsnAsync_Excludes_SoftDeleted_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, hsnId: 99, isDeleted: true);

            var result = await CreateRepo().HasLinkedHsnAsync(99);

            // NOTE: RfqItem branch has no IsDeleted filter — only QuotationDetail has the filter.
            // Since we didn't seed an RfqItem for HsnId=99, this should return false.
            result.Should().BeFalse();
        }

        // ── HasActiveHsnAsync ────────────────────────────────────────────────

        [Fact]
        public async Task HasActiveHsnAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasActiveHsnAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveHsnAsync_Returns_True_When_Active_QuotationDetail_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, hsnId: 55);

            var result = await CreateRepo().HasActiveHsnAsync(55);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveHsnAsync_Excludes_Inactive_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, hsnId: 55, isActive: false);

            var result = await CreateRepo().HasActiveHsnAsync(55);

            // RfqItem also not seeded for HsnId=55 → false
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveHsnAsync_Returns_True_When_Referenced_By_RfqItem_Only()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, hsnId: 88);

            // RfqItem branch has no IsActive filter
            var result = await CreateRepo().HasActiveHsnAsync(88);

            result.Should().BeTrue();
        }
    }
}
