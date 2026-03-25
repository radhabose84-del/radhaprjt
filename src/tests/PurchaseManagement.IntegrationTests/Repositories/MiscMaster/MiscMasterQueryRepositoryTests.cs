using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private MiscMasterCommandRepository CreateCommandRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private MiscTypeMasterCommandRepository CreateMiscTypeCommandRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "TYP001",
                Description = "Test Type",
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
            var result = await CreateMiscTypeCommandRepo(ctx).CreateAsync(entity);
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int miscTypeId, string code = "MC001", string description = "Test Misc")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
            var result = await CreateCommandRepo(ctx).CreateAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscTypeMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedEntityAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Correct_Fields()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedEntityAsync(miscTypeId, "MC001", "Test Misc Master");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items[0].Code.Should().Be("MC001");
            items[0].Description.Should().Be("Test Misc Master");
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var id = await SeedEntityAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, entity);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedEntityAsync(miscTypeId, "ALPHA01", "First Item");
            await SeedEntityAsync(miscTypeId, "BETA001", "Second Item");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("ALPHA01");
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Multiple_Records()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedEntityAsync(miscTypeId, "MC001", "First");
            await SeedEntityAsync(miscTypeId, "MC002", "Second");
            await SeedEntityAsync(miscTypeId, "MC003", "Third");

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(3);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var id = await SeedEntityAsync(miscTypeId, "MC001", "Unique Description");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Code.Should().Be("MC001");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Id.Should().Be(0);
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            await SeedEntityAsync(miscTypeId, "MC001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MC001", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NONEXISTENT", 1);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var id = await SeedEntityAsync(miscTypeId, "MC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted
            };
            await CreateCommandRepo(ctx).DeleteAsync(id, entity);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MC001", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var id = await SeedEntityAsync(miscTypeId, "MC001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MC001", miscTypeId, id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Record_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var id = await SeedEntityAsync(miscTypeId);

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Record_Not_Found()
        {
            await ClearTablesAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }
    }
}
