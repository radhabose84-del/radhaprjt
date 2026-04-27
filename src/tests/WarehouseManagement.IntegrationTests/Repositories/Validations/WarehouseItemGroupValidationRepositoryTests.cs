using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Validations;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class WarehouseItemGroupValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseItemGroupValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseItemGroupValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WarehouseItemGroupValidationRepository(conn);
        }

        private async Task<int> SeedWarehouseAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wh = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = "WH-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                WarehouseName = "Test WH",
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                Pincode = "600001",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.WarehouseMasters.Add(wh);
            await ctx.SaveChangesAsync();
            return wh.Id;
        }

        private async Task SeedMappingAsync(
            int warehouseId,
            int itemGroupId,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mapping = new WarehouseManagement.Domain.Entities.WarehouseItemGroupMapping
            {
                WarehouseId = warehouseId,
                ItemGroupId = itemGroupId,
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.GetWarehouseItemGroupMappings.Add(mapping);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedItemGroupAsync ---

        [Fact]
        public async Task HasLinkedItemGroupAsync_Should_Return_True_When_Mapping_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 100);

            var result = await CreateRepo().HasLinkedItemGroupAsync(100);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemGroupAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedItemGroupAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemGroupAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 200, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedItemGroupAsync(200);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemGroupAsync_Should_Return_True_When_Inactive_But_NotDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 250, active: Status.Inactive);

            var result = await CreateRepo().HasLinkedItemGroupAsync(250);

            result.Should().BeTrue();
        }

        // --- HasActiveItemGroupAsync ---

        [Fact]
        public async Task HasActiveItemGroupAsync_Should_Return_True_When_Mapping_Active()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 300, active: Status.Active);

            var result = await CreateRepo().HasActiveItemGroupAsync(300);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemGroupAsync_Should_Return_False_When_Mapping_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 400, active: Status.Inactive);

            var result = await CreateRepo().HasActiveItemGroupAsync(400);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemGroupAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var whId = await SeedWarehouseAsync();
            await SeedMappingAsync(whId, itemGroupId: 500, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasActiveItemGroupAsync(500);

            result.Should().BeFalse();
        }
    }
}
