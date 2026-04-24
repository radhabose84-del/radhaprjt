using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseItemValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseItemValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PurchaseItemValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PurchaseItemValidationRepository(conn);
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

        private async Task SeedRfqItemAsync(int rfqId, int itemId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.RfqItems.Add(new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqItem
            {
                RfqId = rfqId,
                ItemId = itemId,
                HsnId = 1,
                Quantity = 10m,
                UomId = 1
            });
            await ctx.SaveChangesAsync();
        }

        private async Task<int> SeedIndentHeaderAsync()
        {
            var miscId = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var header = new PurchaseManagement.Domain.Entities.IndentHeader
            {
                IndentNumber = "IND-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                IndentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IndentTypeId = miscId,
                UnitId = 1,
                Purpose = "Test",
                DepartmentId = 1,
                StatusId = miscId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.IndentHeader.Add(header);
            await ctx.SaveChangesAsync();
            return header.Id;
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
            int headerId, int itemId,
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.QuotationDetails.Add(new PurchaseManagement.Domain.Entities.Quotation.QuotationEntry.QuotationDetail
            {
                QuotationHeaderId = headerId,
                ItemId = itemId,
                HsnId = 1,
                Quantity = 10m,
                UomId = 1,
                CurrencyId = 1,
                Rate = 100m,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task SeedIndentDetailAsync(
            int headerId, int itemId,
            bool isActive = true, bool isDeleted = false)
        {
            var miscId = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.IndentDetail.Add(new PurchaseManagement.Domain.Entities.IndentDetail
            {
                IndentHeaderId = headerId,
                ItemId = itemId,
                ItemCategoryId = 1,
                ItemUOMId = 1,
                QuantityRequired = 10m,
                RequiredDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                PRConsumptionDays = 7,
                Remark = "Test",
                StatusId = miscId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedItemAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Returns_True_When_Referenced_By_RfqItem()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, itemId: 42);

            var result = await CreateRepo().HasLinkedItemAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Returns_True_When_Referenced_By_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemId: 77);

            var result = await CreateRepo().HasLinkedItemAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Excludes_SoftDeleted_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemId: 99, isDeleted: true);

            // IndentDetail branch has IsDeleted filter; RfqItem not seeded for 99 → false overall
            var result = await CreateRepo().HasLinkedItemAsync(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemAsync_Returns_True_When_Referenced_By_RfqItem_Only()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, itemId: 88);

            // RfqItem branch has no IsActive filter
            var result = await CreateRepo().HasActiveItemAsync(88);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemAsync_Excludes_Inactive_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemId: 66, isActive: false);

            // IndentDetail has IsActive filter; no RfqItem for 66 → false
            var result = await CreateRepo().HasActiveItemAsync(66);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Returns_True_When_Referenced_By_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, itemId: 111);

            var result = await CreateRepo().HasLinkedItemAsync(111);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Excludes_SoftDeleted_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, itemId: 222, isDeleted: true);

            var result = await CreateRepo().HasLinkedItemAsync(222);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemAsync_Returns_True_When_Referenced_By_Active_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, itemId: 333);

            var result = await CreateRepo().HasActiveItemAsync(333);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemAsync_Excludes_Inactive_QuotationDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            var qhId = await SeedQuotationHeaderAsync(rfqId);
            await SeedQuotationDetailAsync(qhId, itemId: 444, isActive: false);

            var result = await CreateRepo().HasActiveItemAsync(444);

            result.Should().BeFalse();
        }
    }
}
