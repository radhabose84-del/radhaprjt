using Contracts.Interfaces.Validations.MaintenanceManagement;
using Contracts.Interfaces.Validations.ProductionManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.WarehouseManagement;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Repositories.UOMs;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.UOMsTests
{
    [Collection("DatabaseCollection")]
    public sealed class UOMQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMQueryRepository CreateRepo(
            Mock<ISalesUomValidation>? sales = null,
            Mock<IPurchaseUomValidation>? purchase = null,
            Mock<IMaintenanceUomValidation>? maintenance = null,
            Mock<IWarehouseUomValidation>? warehouse = null,
            Mock<IProductionUomValidation>? production = null)
        {
            if (sales == null)
            {
                sales = new Mock<ISalesUomValidation>(MockBehavior.Loose);
                sales.Setup(s => s.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(false);
                sales.Setup(s => s.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (purchase == null)
            {
                purchase = new Mock<IPurchaseUomValidation>(MockBehavior.Loose);
                purchase.Setup(p => p.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(false);
                purchase.Setup(p => p.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (maintenance == null)
            {
                maintenance = new Mock<IMaintenanceUomValidation>(MockBehavior.Loose);
                maintenance.Setup(m => m.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(false);
                maintenance.Setup(m => m.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (warehouse == null)
            {
                warehouse = new Mock<IWarehouseUomValidation>(MockBehavior.Loose);
                warehouse.Setup(w => w.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(false);
                warehouse.Setup(w => w.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(false);
            }
            if (production == null)
            {
                production = new Mock<IProductionUomValidation>(MockBehavior.Loose);
                production.Setup(p => p.HasLinkedUomAsync(It.IsAny<int>())).ReturnsAsync(false);
                production.Setup(p => p.HasActiveUomAsync(It.IsAny<int>())).ReturnsAsync(false);
            }

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UOMQueryRepository(conn, sales.Object, purchase.Object,
                maintenance.Object, warehouse.Object, production.Object);
        }

        private async Task<int> EnsureUomTypeMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "UOM_Q_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UOM_Q_T", Description = "UOM Q Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "UQ_CNT");
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = "UQ_CNT", Description = "QryCount",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<int> SeedUomAsync(
            string code,
            string? name = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            var typeId = await EnsureUomTypeMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var maxOrder = await ctx.UOMs.AnyAsync() ? await ctx.UOMs.MaxAsync(u => u.SortOrder) : 0;
            var u = new UOM
            {
                Code = code,
                UOMName = name ?? code,
                UOMTypeId = typeId,
                SortOrder = maxOrder + 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.UOMs.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllUOMAsync ---

        [Fact]
        public async Task GetAllUOMAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedUomAsync("QA1");

            var (rows, total) = await CreateRepo().GetAllUOMAsync(1, 10, null!);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedUomAsync("QDEL", deleted: IsDelete.Deleted);

            var (rows, _) = await CreateRepo().GetAllUOMAsync(1, 10, "QDEL");

            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedUomAsync("UQSEARCH1", "Searchable");
            await SeedUomAsync("QOTHER2", "Other");

            var (rows, _) = await CreateRepo().GetAllUOMAsync(1, 10, "UQSEARCH");

            rows.Should().OnlyContain(r => r.Code == "UQSEARCH1");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedUomAsync("GID1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Code.Should().Be("GID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedUomAsync("GSD1", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GetByUOMNameAsync ---

        [Fact]
        public async Task GetByUOMNameAsync_Should_Return_Match()
        {
            await ClearAsync();
            await SeedUomAsync("UN1", "UniqueName");

            var result = await CreateRepo().GetByUOMNameAsync("UniqueName");

            result.Should().NotBeNull();
            result.UOMName.Should().Be("UniqueName");
        }

        [Fact]
        public async Task GetByUOMNameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearAsync();
            var id = await SeedUomAsync("SN1", "SelfName");

            var result = await CreateRepo().GetByUOMNameAsync("SelfName", id);

            result.Should().BeNull();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedUomAsync("NF1");

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
            var id = await SeedUomAsync("SDV1");

            var result = await CreateRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearAsync();
            var id = await SeedUomAsync("SDV2");
            var sales = new Mock<ISalesUomValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedUomAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(sales: sales).SoftDeleteValidation(id);

            result.Should().BeTrue();
        }

        // --- IsUOMLinkedAsync ---

        [Fact]
        public async Task IsUOMLinkedAsync_Should_Return_False_When_Not_Linked()
        {
            await ClearAsync();
            var id = await SeedUomAsync("LK1");

            var result = await CreateRepo().IsUOMLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsUOMLinkedAsync_Should_Return_True_When_PurchaseValidation_Active()
        {
            await ClearAsync();
            var id = await SeedUomAsync("LK2");
            var purchase = new Mock<IPurchaseUomValidation>(MockBehavior.Loose);
            purchase.Setup(p => p.HasActiveUomAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(purchase: purchase).IsUOMLinkedAsync(id);

            result.Should().BeTrue();
        }

        // --- GetUOMAsync (all) ---

        [Fact]
        public async Task GetUOMAsync_Should_Return_All_Non_Deleted()
        {
            await ClearAsync();
            await SeedUomAsync("GA1");
            await SeedUomAsync("GA2");
            await SeedUomAsync("GADEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetUOMAsync();

            result.Should().HaveCount(2);
        }

        // --- GetUOM (search) ---

        [Fact]
        public async Task GetUOM_Should_Filter_By_Pattern()
        {
            await ClearAsync();
            await SeedUomAsync("PP1", "Pickled");
            await SeedUomAsync("PP2", "Plain");

            var result = await CreateRepo().GetUOM("Pick");

            result.Should().ContainSingle();
            result[0].UOMName.Should().Be("Pickled");
        }

        // --- GetUOM with uomTypeCode filter (only UOMs under the given misc category) ---

        private async Task<int> EnsureMiscByCodeAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "UOM_Q_T");
            if (type == null)
            {
                type = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UOM_Q_T", Description = "UOM Q Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code);
            if (misc == null)
            {
                misc = new InventoryManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = type.Id, Code = code, Description = code,
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private async Task<int> SeedUomUnderMiscAsync(string code, string name, int miscId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var maxOrder = await ctx.UOMs.AnyAsync() ? await ctx.UOMs.MaxAsync(u => u.SortOrder) : 0;
            var u = new UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = miscId,
                SortOrder = maxOrder + 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.UOMs.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        [Fact]
        public async Task GetUOM_With_UOMTypeCode_Should_Return_Only_Matching_Category()
        {
            await ClearAsync();
            var volumeId = await EnsureMiscByCodeAsync("VolUnits");
            var weightId = await EnsureMiscByCodeAsync("WgtUnits");

            await SeedUomUnderMiscAsync("VL_LTR", "Litres", volumeId);
            await SeedUomUnderMiscAsync("VL_ML", "Millilitre", volumeId);
            await SeedUomUnderMiscAsync("WT_KG", "Kilogram", weightId);

            var result = await CreateRepo().GetUOM(string.Empty, "VolUnits");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(u => u.UOMName == "Litres" || u.UOMName == "Millilitre");
        }

        [Fact]
        public async Task GetUOM_Without_UOMTypeCode_Should_Return_All_Categories()
        {
            await ClearAsync();
            var volumeId = await EnsureMiscByCodeAsync("VolUnits");
            var weightId = await EnsureMiscByCodeAsync("WgtUnits");

            await SeedUomUnderMiscAsync("VL_LTR2", "Litres2", volumeId);
            await SeedUomUnderMiscAsync("WT_KG2", "Kilogram2", weightId);

            var result = await CreateRepo().GetUOM(string.Empty);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUOM_With_UOMTypeCode_Should_Combine_With_Search_Pattern()
        {
            await ClearAsync();
            var volumeId = await EnsureMiscByCodeAsync("VolUnits");

            await SeedUomUnderMiscAsync("VL_LTR3", "Litres3", volumeId);
            await SeedUomUnderMiscAsync("VL_ML3", "Millilitre3", volumeId);

            var result = await CreateRepo().GetUOM("Litres", "VolUnits");

            result.Should().ContainSingle();
            result[0].UOMName.Should().Be("Litres3");
        }
    }
}
