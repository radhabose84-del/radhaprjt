using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.Validations;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseItemCategoryValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PurchaseItemCategoryValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PurchaseItemCategoryValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PurchaseItemCategoryValidationRepository(conn);
        }

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync();
            if (existing != null) return existing.Id;

            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "IndentType",
                Description = "Indent Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "Standard",
                Description = "Standard",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
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
            int headerId, int itemCategoryId,
            bool isActive = true, bool isDeleted = false)
        {
            var miscId = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.IndentDetail.Add(new PurchaseManagement.Domain.Entities.IndentDetail
            {
                IndentHeaderId = headerId,
                ItemId = 1,
                ItemCategoryId = itemCategoryId,
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
        public async Task HasLinkedItemCategoryAsync_Returns_False_When_NotReferenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedItemCategoryAsync(9999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemCategoryAsync_Returns_True_When_Referenced_By_IndentDetail()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemCategoryId: 42);

            var result = await CreateRepo().HasLinkedItemCategoryAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemCategoryAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemCategoryId: 42, isDeleted: true);

            var result = await CreateRepo().HasLinkedItemCategoryAsync(42);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemCategoryAsync_Returns_True_When_Active()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemCategoryId: 55);

            var result = await CreateRepo().HasActiveItemCategoryAsync(55);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemCategoryAsync_Returns_False_When_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var headerId = await SeedIndentHeaderAsync();
            await SeedIndentDetailAsync(headerId, itemCategoryId: 55, isActive: false);

            var result = await CreateRepo().HasActiveItemCategoryAsync(55);

            result.Should().BeFalse();
        }
    }
}
