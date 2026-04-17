using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.SalesManagement;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Repositories.HSNMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.HSNMasterTests
{
    [Collection("DatabaseCollection")]
    public sealed class HSNMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HSNMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private HSNMasterQueryRepository CreateRepo(
            Mock<ISalesHsnValidation>? sales = null,
            Mock<IPurchaseHsnValidation>? purchase = null)
        {
            if (sales == null)
            {
                sales = new Mock<ISalesHsnValidation>(MockBehavior.Loose);
                sales.Setup(s => s.HasLinkedHsnAsync(It.IsAny<int>())).ReturnsAsync(false);
                sales.Setup(s => s.HasActiveHsnAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (purchase == null)
            {
                purchase = new Mock<IPurchaseHsnValidation>(MockBehavior.Loose);
                purchase.Setup(p => p.HasLinkedHsnAsync(It.IsAny<int>())).ReturnsAsync(false);
                purchase.Setup(p => p.HasActiveHsnAsync(It.IsAny<int>())).ReturnsAsync(false);
            }

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new HSNMasterQueryRepository(conn, sales.Object, purchase.Object);
        }

        private async Task<(int MiscId, int TypeId)> EnsureHsnTypeMiscAsync(string miscCode = "HSN")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "HSNType");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "HSNType", Description = "HSN Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == miscCode);
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = miscCode, Description = miscCode,
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return (misc.Id, type.Id);
        }

        private async Task<int> SeedHSNAsync(
            string code = "3001",
            string? description = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            var (miscId, _) = await EnsureHsnTypeMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new HSNMaster
            {
                HSNCode = code,
                Description = description ?? $"HSN {code}",
                TypeId = miscId,
                GSTCategoryId = miscId,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.HSNMaster.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_TotalCount()
        {
            await ClearAsync();
            await SeedHSNAsync("QR1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null!);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedHSNAsync("UNIQHSN1", "UniqueDesc1");
            await SeedHSNAsync("OTHER2", "Other");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQHSN");

            rows.Should().OnlyContain(r => r.HSNCode == "UNIQHSN1");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedHSNAsync("QRDEL1", deleted: IsDelete.Deleted);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "QRDEL1");

            rows.Should().BeEmpty();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("QRID1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.HSNCode.Should().Be("QRID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("DELID1", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AlreadyExistsAsync ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedHSNAsync("DUPX1");

            var exists = await CreateRepo().AlreadyExistsAsync("DUPX1");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("SELFX1");

            var exists = await CreateRepo().AlreadyExistsAsync("SELFX1", id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_Unknown()
        {
            var exists = await CreateRepo().AlreadyExistsAsync("ZZNOMATCHZZ");
            exists.Should().BeFalse();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("NFX1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        // --- SoftDeleteValidation ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("SDV1");

            var result = await CreateRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("SDV2");
            var sales = new Mock<ISalesHsnValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedHsnAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(sales: sales).SoftDeleteValidation(id);

            result.Should().BeTrue();
        }

        // --- IsHSNMasterLinkedAsync ---

        [Fact]
        public async Task IsHSNMasterLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("LK1");

            var result = await CreateRepo().IsHSNMasterLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsHSNMasterLinkedAsync_Should_Return_True_When_PurchaseValidation_Active()
        {
            await ClearAsync();
            var id = await SeedHSNAsync("LK2");
            var purchase = new Mock<IPurchaseHsnValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasActiveHsnAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(purchase: purchase).IsHSNMasterLinkedAsync(id);

            result.Should().BeTrue();
        }
    }
}
