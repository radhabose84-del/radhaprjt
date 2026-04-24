using Microsoft.Data.SqlClient;
using WarehouseManagement.Infrastructure.Repositories.Validations;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.IntegrationTests.Repositories.Validations
{
    [Collection("DatabaseCollection")]
    public sealed class WarehouseDepartmentValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseDepartmentValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseDepartmentValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WarehouseDepartmentValidationRepository(conn);
        }

        private async Task SeedWarehouseAsync(
            int departmentId,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var wh = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = "WH-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                WarehouseName = $"Test WH Dep {departmentId}",
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = departmentId,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                Pincode = "600001",
                IsActive = active,
                IsDeleted = deleted
            };
            ctx.WarehouseMasters.Add(wh);
            await ctx.SaveChangesAsync();
        }

        // --- HasLinkedDepartmentAsync ---

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_Warehouse_Uses_DepartmentId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 10);

            var result = await CreateRepo().HasLinkedDepartmentAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedDepartmentAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 20, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasLinkedDepartmentAsync(20);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedDepartmentAsync_Should_Return_True_When_Inactive_But_NotDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 25, active: Status.Inactive);

            var result = await CreateRepo().HasLinkedDepartmentAsync(25);

            result.Should().BeTrue();
        }

        // --- HasActiveDepartmentAsync ---

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_True_When_Warehouse_Is_Active()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 30, active: Status.Active);

            var result = await CreateRepo().HasActiveDepartmentAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_Warehouse_Is_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 40, active: Status.Inactive);

            var result = await CreateRepo().HasActiveDepartmentAsync(40);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveDepartmentAsync_Should_Return_False_When_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedWarehouseAsync(departmentId: 50, deleted: IsDelete.Deleted);

            var result = await CreateRepo().HasActiveDepartmentAsync(50);

            result.Should().BeFalse();
        }
    }
}
