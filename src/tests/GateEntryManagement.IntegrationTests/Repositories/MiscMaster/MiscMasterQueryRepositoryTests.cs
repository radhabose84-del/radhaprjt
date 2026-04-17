using Microsoft.Data.SqlClient;
using Dapper;
using GateEntryManagement.Infrastructure.Repositories.MiscMaster;
using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeAsync(string miscTypeCode = "TESTTYPE", string description = "Test Type")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedMiscMasterAsync(
            int miscTypeId,
            string code = "MISC001",
            string description = "Test Misc",
            int sortOrder = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            return await repo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId, "ALPHA01", "Alpha Misc");
            await SeedMiscMasterAsync(miscTypeId, "BETA001", "Beta Misc");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Misc");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearTableAsync();
            var miscTypeId1 = await SeedMiscTypeAsync("TYPE001", "Type One");
            var miscTypeId2 = await SeedMiscTypeAsync("TYPE002", "Type Two");
            await SeedMiscMasterAsync(miscTypeId1, "MISC001", "Misc One");
            await SeedMiscMasterAsync(miscTypeId2, "MISC002", "Misc Two");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, miscTypeId1);

            items.Should().HaveCount(1);
            items[0].MiscTypeId.Should().Be(miscTypeId1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync("VTYPE01", "Vehicle Type");
            var id = await SeedMiscMasterAsync(miscTypeId, "MISC001", "Test Misc", 3);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Code.Should().Be("MISC001");
            dto.Description.Should().Be("Test Misc");
            dto.MiscTypeId.Should().Be(miscTypeId);
            dto.MiscTypeCode.Should().Be("VTYPE01");
            dto.SortOrder.Should().Be(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedMiscMasterAsync(miscTypeId, "MISC001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MISC001", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId, "MISC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MISC001", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_When_Inactive()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == miscTypeId);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedMiscMasterAsync(miscTypeId, "MISC001", "Active Misc");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Active", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_MiscTypeCode()
        {
            await ClearTableAsync();
            var miscTypeId1 = await SeedMiscTypeAsync("TYPE001", "Type One");
            var miscTypeId2 = await SeedMiscTypeAsync("TYPE002", "Type Two");
            await SeedMiscMasterAsync(miscTypeId1, "MISC001", "Misc One");
            await SeedMiscMasterAsync(miscTypeId2, "MISC002", "Misc Two");

            var results = await CreateQueryRepo().AutocompleteAsync("", "TYPE001", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].MiscTypeCode.Should().Be("TYPE001");
        }
    }
}
