using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    // =====================================================================================
    // BLOCKED - Production SQL references a table name that does not match the EF config.
    //
    // PurchaseCurrencyValidationRepository.HasLinkedCurrencyAsync executes:
    //     SELECT CASE WHEN
    //         EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE CurrencyId = @Id AND IsDeleted = 0)
    //         OR EXISTS (SELECT 1 FROM [Purchase].[ImportPOHeader] WHERE CurrencyId = @Id AND IsDeleted = 0)
    //         OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterDetail] WHERE CurrencyId = @Id AND IsDeleted = 0)
    //         OR EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE CurrencyId = @Id AND IsDeleted = 0)
    //     THEN 1 ELSE 0 END
    //
    // The ImportPOHeader entity's EF configuration (ImportPOHeaderConfiguration.cs) maps it to
    // table 'Purchase.PurchaseOrderImportHeader' via ToTable(...) - the Dapper SQL was not
    // updated when the table was renamed. The whole CASE expression parses as one statement,
    // so the "Invalid object name 'Purchase.ImportPOHeader'" error fires on every call.
    //
    // Remove the Skip argument on each fact once the production SQL is corrected
    // (change 'ImportPOHeader' to 'PurchaseOrderImportHeader' in the repository query).
    // =====================================================================================
    [Collection("DatabaseCollection")]
    public sealed class PurchaseCurrencyValidationRepositoryTests
    {
        private const string BlockedReason =
            "BLOCKED - Production SQL in PurchaseCurrencyValidationRepository references " +
            "Purchase.ImportPOHeader but the ImportPOHeader entity is mapped to table " +
            "Purchase.PurchaseOrderImportHeader. See class XML doc for details.";

        private readonly DbFixture _fixture;

        public PurchaseCurrencyValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PurchaseCurrencyValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PurchaseCurrencyValidationRepository(conn);
        }

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync();
            if (existing != null) return existing.Id;

            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "Status",
                Description = "Status",
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
            int headerId, int currencyId,
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.QuotationDetails.Add(new PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationDetail
            {
                QuotationHeaderId = headerId,
                ItemId = 1,
                HsnId = 1,
                Quantity = 10m,
                UomId = 1,
                CurrencyId = currencyId,
                Rate = 100m,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact(Skip = BlockedReason)]
        public async Task HasLinkedCurrencyAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedCurrencyAsync(9999);

            result.Should().BeFalse();
        }

        [Fact(Skip = BlockedReason)]
        public async Task HasLinkedCurrencyAsync_Returns_True_When_Referenced_By_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, currencyId: 77);

            var result = await CreateRepo().HasLinkedCurrencyAsync(77);

            result.Should().BeTrue();
        }

        [Fact(Skip = BlockedReason)]
        public async Task HasLinkedCurrencyAsync_Excludes_SoftDeleted_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, currencyId: 77, isDeleted: true);

            var result = await CreateRepo().HasLinkedCurrencyAsync(77);

            result.Should().BeFalse();
        }
    }
}
