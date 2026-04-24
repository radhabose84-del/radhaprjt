using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseUomValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseUomValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PurchaseUomValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PurchaseUomValidationRepository(conn);
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

        private async Task SeedRfqItemAsync(int rfqId, int uomId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.RfqItems.Add(new PurchaseManagement.Domain.Entities.Quotation.RfqEntry.RfqItem
            {
                RfqId = rfqId,
                ItemId = 1,
                HsnId = 1,
                Quantity = 10m,
                UomId = uomId
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

        private async Task SeedIndentDetailAsync(
            int headerId, int itemUomId,
            bool isActive = true, bool isDeleted = false)
        {
            var miscId = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.IndentDetail.Add(new PurchaseManagement.Domain.Entities.IndentDetail
            {
                IndentHeaderId = headerId,
                ItemId = 1,
                ItemCategoryId = 1,
                ItemUOMId = itemUomId,
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
        public async Task HasLinkedUomAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedUomAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Returns_True_When_Referenced_By_RfqItem()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, uomId: 42);

            var result = await CreateRepo().HasLinkedUomAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Returns_True_When_Referenced_By_IndentDetail_ItemUOMId()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemUomId: 77);

            var result = await CreateRepo().HasLinkedUomAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Excludes_SoftDeleted_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemUomId: 99, isDeleted: true);

            var result = await CreateRepo().HasLinkedUomAsync(99);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Returns_True_When_Referenced_By_RfqItem_Only()
        {
            await _fixture.ClearAllTablesAsync();
            var rfqId = await SeedRfqMasterAsync();
            await SeedRfqItemAsync(rfqId, uomId: 88);

            var result = await CreateRepo().HasActiveUomAsync(88);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveUomAsync_Excludes_Inactive_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemUomId: 66, isActive: false);

            var result = await CreateRepo().HasActiveUomAsync(66);

            result.Should().BeFalse();
        }
    }
}
