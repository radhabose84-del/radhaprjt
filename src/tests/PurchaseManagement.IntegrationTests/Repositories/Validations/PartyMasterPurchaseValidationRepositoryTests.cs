using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PartyMasterPurchaseValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyMasterPurchaseValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyMasterPurchaseValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyMasterPurchaseValidationRepository(conn);
        }

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

        private async Task SeedRfqSupplierAsync(int rfqId, int supplierId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.RfqSuppliers.Add(new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqSupplier
            {
                RfqId = rfqId,
                SupplierId = supplierId,
                Name = "Test Supplier"
            });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> SeedQuotationHeaderAsync(
            int rfqId, int supplierId,
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var header = new PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationHeader
            {
                UnitId = 1,
                RfqId = rfqId,
                SupplierId = supplierId,
                QuotationNumber = "Q-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                ValidTill = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.QuotationHeaders.Add(header);
            await ctx.SaveChangesAsync();
            return header.Id;
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedPartyMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Returns_True_When_Referenced_By_RfqSupplier()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqSupplierAsync(rfqId, supplierId: 42);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Returns_True_When_Referenced_By_RfqSupplier()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqSupplierAsync(rfqId, supplierId: 55);

            // RfqSuppliers branch has no IsDeleted/IsActive filter — always true when row exists
            var result = await CreateRepo().HasActivePartyMasterAsync(55);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasActivePartyMasterAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Returns_True_When_Referenced_By_QuotationHeader_Supplier()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedQuotationHeaderAsync(rfqId, supplierId: 123);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(123);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedPartyMasterAsync_Excludes_SoftDeleted_QuotationHeader()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedQuotationHeaderAsync(rfqId, supplierId: 456, isDeleted: true);

            var result = await CreateRepo().HasLinkedPartyMasterAsync(456);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Returns_True_When_Referenced_By_QuotationHeader_Supplier()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedQuotationHeaderAsync(rfqId, supplierId: 789);

            var result = await CreateRepo().HasActivePartyMasterAsync(789);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActivePartyMasterAsync_Excludes_Inactive_QuotationHeader()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedQuotationHeaderAsync(rfqId, supplierId: 321, isActive: false);

            var result = await CreateRepo().HasActivePartyMasterAsync(321);

            result.Should().BeFalse();
        }
    }
}
