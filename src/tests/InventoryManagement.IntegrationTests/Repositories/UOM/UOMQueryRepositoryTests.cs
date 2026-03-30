using Dapper;
using Microsoft.Data.SqlClient;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using InventoryManagement.Infrastructure.Repositories.UOMs;

namespace InventoryManagement.IntegrationTests.Repositories.UOM
{
    [Collection("DatabaseCollection")]
    public sealed class UOMQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new UOMQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test UOM Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "UOM Type Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int uomTypeId, string code = "QRY_UOM", string uomName = "Query UOM")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new UOMCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Code = code,
                UOMName = uomName,
                UOMTypeId = uomTypeId,
                SortOrder = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOM]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllUOMAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q1");
            await SeedEntityAsync(miscId, "PC", "Piece");

            var (items, total) = await CreateQueryRepo().GetAllUOMAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q2");
            var id = await SeedEntityAsync(miscId, "DL", "Delete UOM");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMCommandRepository(ctx).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllUOMAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q3");
            await SeedEntityAsync(miscId, "KG", "Kilogram");
            await SeedEntityAsync(miscId, "LT", "Litre");

            var (items, _) = await CreateQueryRepo().GetAllUOMAsync(1, 10, "KG");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("KG");
        }

        [Fact]
        public async Task GetAllUOMAsync_Should_Return_Correct_PaginationMetadata()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T4");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q4");
            await SeedEntityAsync(miscId, "P1", "Page UOM 1");
            await SeedEntityAsync(miscId, "P2", "Page UOM 2");

            var (items, total) = await CreateQueryRepo().GetAllUOMAsync(1, 1, null);

            items.Should().HaveCount(1);
            total.Should().Be(2);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T5");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q5");
            var id = await SeedEntityAsync(miscId, "MT", "Meter");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("MT");
            result.UOMName.Should().Be("Meter");
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
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T6");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q6");
            var id = await SeedEntityAsync(miscId, "SD", "Soft Del UOM");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMCommandRepository(ctx).DeleteAsync(id,
                new InventoryManagement.Domain.Entities.UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GET BY UOM NAME ---

        [Fact]
        public async Task GetByUOMNameAsync_Should_Return_Entity_When_Found()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T7");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q7");
            await SeedEntityAsync(miscId, "NM", "Nanometer");

            var result = await CreateQueryRepo().GetByUOMNameAsync("Nanometer");

            result.Should().NotBeNull();
            result!.UOMName.Should().Be("Nanometer");
        }

        [Fact]
        public async Task GetByUOMNameAsync_Should_Return_Null_When_Not_Found()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByUOMNameAsync("DoesNotExist");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUOMNameAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UOM_QRY_T8");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UOM_Q8");
            var id = await SeedEntityAsync(miscId, "EM", "Exclusive Match");

            var result = await CreateQueryRepo().GetByUOMNameAsync("Exclusive Match", id);

            result.Should().BeNull();
        }
    }
}
