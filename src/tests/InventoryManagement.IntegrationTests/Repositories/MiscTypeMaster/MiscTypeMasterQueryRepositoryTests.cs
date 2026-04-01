using Dapper;
using Microsoft.Data.SqlClient;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace InventoryManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "INV_TYPE", string description = "Inventory Type")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var entity = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[IssueDetail]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[IssueHeader]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MrsDetail]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MrsHeader]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOMConversion]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOM]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Correct_Fields()
        {
            await ClearTableAsync();
            await SeedEntityAsync("INV_TYPE", "Inventory Type");

            var (items, _) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items[0].MiscTypeCode.Should().Be("INV_TYPE");
            items[0].Description.Should().Be("Inventory Type");
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(id, toDelete);

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ALPHA_TYPE", "Alpha");
            await SeedEntityAsync("BETA_TYPE", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("ALPHA_TYPE");
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Return_Multiple_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPE_A", "Type A");
            await SeedEntityAsync("TYPE_B", "Type B");
            await SeedEntityAsync("TYPE_C", "Type C");

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().HaveCount(3);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("INV_TYPE", "Inventory Type");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.MiscTypeCode.Should().Be("INV_TYPE");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("INV_TYPE");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("INV_TYPE");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NONEXISTENT");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("INV_TYPE");

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(id, toDelete);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("INV_TYPE");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("INV_TYPE");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("INV_TYPE", id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Record_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Record_Not_Found()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }

        // --- GET MISC TYPE MASTER (autocomplete) ---

        [Fact]
        public async Task GetMiscTypeMaster_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("INV_TYPE", "Inventory Type");
            await SeedEntityAsync("INV_STATUS", "Inventory Status");
            await SeedEntityAsync("OTHER", "Other");

            var results = await CreateQueryRepo().GetMiscTypeMaster("INV");

            results.Should().HaveCount(2);
        }
    }
}
