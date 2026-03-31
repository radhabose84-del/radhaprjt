using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Infrastructure.Repositories.WarehouseMaster;

namespace WarehouseManagement.IntegrationTests.Repositories.WarehouseMaster
{
    [Collection("DatabaseCollection")]
    public sealed class WarehouseMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WarehouseMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WarehouseMasterQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WarehouseMasterQueryRepository(conn, ipMock.Object);
        }

        private WarehouseMasterCommandRepository CreateCommandRepo(WarehouseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var logger = new Mock<ILogger<WarehouseMasterCommandRepository>>(MockBehavior.Loose).Object;
            return new WarehouseMasterCommandRepository(ctx, logger);
        }

        private async Task<int> SeedEntityAsync(string code = "WH001", string name = "Test Warehouse")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                WarehouseCode = code,
                WarehouseName = name,
                UnitId = 1,
                IsGroup = false,
                IsVirtualWarehouse = false,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "Test Address",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "000000",
                IsScrapWarehouse = false,
                IsTransitWarehouse = false,
                MaxCapacity = 100,
                IsDefaultStockEntry = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Warehouse].[BinMaster]");
            await conn.ExecuteAsync("DELETE FROM [Warehouse].[RackMaster]");
            await conn.ExecuteAsync("DELETE FROM [Warehouse].[WarehouseMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Correct_WarehouseCode()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("WHTEST", "Named Warehouse");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].WarehouseCode.Should().Be("WHTEST");
            items[0].WarehouseName.Should().Be("Named Warehouse");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, toDelete);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("WH001", "Alpha Warehouse");
            await SeedEntityAsync("WH002", "Beta Warehouse");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].WarehouseName.Should().Be("Alpha Warehouse");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Multiple_Records()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("WH001", "Warehouse One");
            await SeedEntityAsync("WH002", "Warehouse Two");
            await SeedEntityAsync("WH003", "Warehouse Three");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(3);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync("WH001", "Query Warehouse");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.WarehouseCode.Should().Be("WH001");
            result.WarehouseName.Should().Be("Query Warehouse");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new WarehouseManagement.Domain.Entities.WarehouseMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, toDelete);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- EXISTS BY NAME ---

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            await SeedEntityAsync("WH001", "Unique Warehouse");

            var exists = await CreateQueryRepo().ExistsByNameAsync("Unique Warehouse");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().ExistsByNameAsync("NonExistentWarehouse");

            exists.Should().BeFalse();
        }
    }
}
