using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.LotMaster
{
    [Collection("DatabaseCollection")]
    public sealed class LotMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LotMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private LotMasterQueryRepository CreateQueryRepo(
            Mock<IItemLookup> itemLookup = null,
            Mock<IUnitLookup> unitLookup = null)
        {
            itemLookup ??= BuildDefaultItemLookup();
            unitLookup ??= BuildDefaultUnitLookup();
            return new LotMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                itemLookup.Object,
                unitLookup.Object);
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup(int itemId = 1, string itemCode = "ITEM01", string itemName = "Test Item")
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new() { Id = itemId, ItemCode = itemCode, ItemName = itemName }
                });
            return mock;
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup(int unitId = 1, string unitName = "Test Unit")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = unitId, UnitName = unitName, ShortName = "TU", UnitHeadName = "Head", OldUnitId = "1" }
                });
            return mock;
        }

        private async Task<(int lotTypeId, int statusId)> SeedPrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "LOTMT",
                Description = "Lot Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscRepo = new MiscMasterCommandRepository(ctx);

            var lotTypeId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LTYPE",
                Description = "Lot Type Desc",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();

            var statusId = await miscRepo.CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "LSTAT",
                Description = "Lot Status Desc",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            return (lotTypeId, statusId);
        }

        private async Task<int> SeedLotMasterAsync(
            int lotTypeId,
            int statusId,
            string lotCode = "LOT001",
            string batchNumber = "BATCH001",
            int itemId = 1,
            int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new LotMasterCommandRepository(ctx).CreateAsync(new Domain.Entities.LotMaster
            {
                LotCode = lotCode,
                BatchNumber = batchNumber,
                LotTypeId = lotTypeId,
                ItemId = itemId,
                UnitId = unitId,
                StartDate = new DateOnly(2026, 1, 15),
                StatusId = statusId,
                ProductionOrderRef = "PO-001",
                TotalProducedQty = 100.0m,
                AvailableQty = 80.0m,
                RunOutDate = new DateOnly(2026, 6, 30),
                Remarks = "Test lot",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[LotMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_LotTypeName_And_StatusName()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].LotTypeName.Should().Be("Lot Type Desc");
            items[0].StatusName.Should().Be("Lot Status Desc");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_ItemName_From_Lookup()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId, itemId: 5);

            var itemMock = BuildDefaultItemLookup(5, "YARN01", "Yarn Item");
            var (items, _) = await CreateQueryRepo(itemMock).GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].ItemCode.Should().Be("YARN01");
            items[0].ItemName.Should().Be("Yarn Item");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_UnitName_From_Lookup()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId, unitId: 3);

            var unitMock = BuildDefaultUnitLookup(3, "Mill Unit");
            var (items, _) = await CreateQueryRepo(unitLookup: unitMock).GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].UnitName.Should().Be("Mill Unit");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new LotMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId, "LOT001", "ALPHA-BATCH");
            await SeedLotMasterAsync(lotTypeId, statusId, "LOT002", "BETA-BATCH");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].BatchNumber.Should().Be("ALPHA-BATCH");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId, "LOT001", "BATCH-BYID");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.LotCode.Should().Be("LOT001");
            dto.BatchNumber.Should().Be("BATCH-BYID");
            dto.LotTypeName.Should().Be("Lot Type Desc");
            dto.StatusName.Should().Be("Lot Status Desc");
            dto.TotalProducedQty.Should().Be(100.0m);
            dto.AvailableQty.Should().Be(80.0m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new LotMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId, "LOT001", "ALPHA");
            await SeedLotMasterAsync(lotTypeId, statusId, "LOT002", "BETA");

            var results = await CreateQueryRepo().AutocompleteAsync("LOT001", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].LotCode.Should().Be("LOT001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId, "LOT001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.LotMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("LOT001", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            await SeedLotMasterAsync(lotTypeId, statusId, "DUPL01");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPL01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId, "DEL01");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new LotMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DEL01");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var (lotTypeId, statusId) = await SeedPrerequisitesAsync();
            var id = await SeedLotMasterAsync(lotTypeId, statusId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- LOT TYPE EXISTS ---

        [Fact]
        public async Task LotTypeExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var (lotTypeId, _) = await SeedPrerequisitesAsync();

            var exists = await CreateQueryRepo().LotTypeExistsAsync(lotTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task LotTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().LotTypeExistsAsync(99999);

            exists.Should().BeFalse();
        }

        // --- STATUS EXISTS ---

        [Fact]
        public async Task StatusExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var (_, statusId) = await SeedPrerequisitesAsync();

            var exists = await CreateQueryRepo().StatusExistsAsync(statusId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().StatusExistsAsync(99999);

            exists.Should().BeFalse();
        }

        // --- ITEM EXISTS (cross-module via lookup) ---

        [Fact]
        public async Task ItemExistsAsync_Should_Return_True_When_Lookup_Returns_Item()
        {
            var itemMock = BuildDefaultItemLookup(1, "ITEM01", "Test Item");

            var exists = await CreateQueryRepo(itemMock).ItemExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Lookup_Returns_Empty()
        {
            var itemMock = new Mock<IItemLookup>(MockBehavior.Loose);
            itemMock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>());

            var exists = await CreateQueryRepo(itemMock).ItemExistsAsync(99999);

            exists.Should().BeFalse();
        }

        // --- UNIT EXISTS (cross-module via lookup) ---

        [Fact]
        public async Task UnitExistsAsync_Should_Return_True_When_Lookup_Returns_Unit()
        {
            var unitMock = BuildDefaultUnitLookup(1, "Test Unit");

            var exists = await CreateQueryRepo(unitLookup: unitMock).UnitExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task UnitExistsAsync_Should_Return_False_When_Lookup_Returns_Empty()
        {
            var unitMock = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitMock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>());

            var exists = await CreateQueryRepo(unitLookup: unitMock).UnitExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
