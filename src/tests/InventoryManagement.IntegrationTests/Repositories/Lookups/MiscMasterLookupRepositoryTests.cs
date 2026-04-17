using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureTypeAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == typeCode);
            if (existing != null) return existing.Id;
            var t = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode, Description = typeCode,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<int> SeedMiscAsync(int typeId, string code, int sortOrder = 1, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = code, Description = code, SortOrder = sortOrder,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearMiscAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetMiscMasterByIdAsync (filters by MiscType code) ---

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Return_Matching_Misc_For_Type()
        {
            await ClearMiscAsync();
            var typeId = await EnsureTypeAsync("MML_TYPE_A");
            await SeedMiscAsync(typeId, "A1");
            await SeedMiscAsync(typeId, "A2");

            var result = await CreateRepo().GetMiscMasterByIdAsync("MML_TYPE_A");

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Return_Empty_When_Type_Not_Found()
        {
            await ClearMiscAsync();

            var result = await CreateRepo().GetMiscMasterByIdAsync("UNKNOWN_TYPE_XYZ");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Exclude_Inactive()
        {
            await ClearMiscAsync();
            var typeId = await EnsureTypeAsync("MML_TYPE_B");
            await SeedMiscAsync(typeId, "ACT");
            await SeedMiscAsync(typeId, "INACT", active: Status.Inactive);

            var result = await CreateRepo().GetMiscMasterByIdAsync("MML_TYPE_B");

            result.Should().Contain(m => m.Code == "ACT");
            result.Should().NotContain(m => m.Code == "INACT");
        }

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Exclude_SoftDeleted()
        {
            await ClearMiscAsync();
            var typeId = await EnsureTypeAsync("MML_TYPE_C");
            await SeedMiscAsync(typeId, "KEEP");
            await SeedMiscAsync(typeId, "DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetMiscMasterByIdAsync("MML_TYPE_C");

            result.Should().Contain(m => m.Code == "KEEP");
            result.Should().NotContain(m => m.Code == "DEL");
        }

        [Fact]
        public async Task GetMiscMasterByIdAsync_Should_Order_By_SortOrder()
        {
            await ClearMiscAsync();
            var typeId = await EnsureTypeAsync("MML_TYPE_D");
            await SeedMiscAsync(typeId, "X", sortOrder: 5);
            await SeedMiscAsync(typeId, "Y", sortOrder: 1);
            await SeedMiscAsync(typeId, "Z", sortOrder: 3);

            var result = await CreateRepo().GetMiscMasterByIdAsync("MML_TYPE_D");

            result.Select(m => m.Code).Should().ContainInOrder("Y", "Z", "X");
        }

        // --- GetMiscTypeIdsAsync ---

        [Fact]
        public async Task GetMiscTypeIdsAsync_Should_Return_Ids_For_Known_Types()
        {
            var warehouseId = await EnsureTypeAsync(MiscEnumEntity.WarehouseType);
            var storageId = await EnsureTypeAsync(MiscEnumEntity.StorageType);
            var areaId = await EnsureTypeAsync(MiscEnumEntity.AreaType);

            var result = await CreateRepo().GetMiscTypeIdsAsync();

            result.WarehouseTypeId.Should().Be(warehouseId);
            result.StorageTypeId.Should().Be(storageId);
            result.AreaTypeId.Should().Be(areaId);
        }
    }
}
