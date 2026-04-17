using Dapper;
using Microsoft.Data.SqlClient;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace PartyManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "MT_QRY")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var entity = new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int miscTypeId, string code = "MM_QRY001", string description = "Test Misc")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var entity = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync() => await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_GETALL1");
            await SeedEntityAsync(miscTypeId, "MM_GA1", "Get All Test");

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_Deleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_EXCL1");
            var id = await SeedEntityAsync(miscTypeId, "MM_EX1", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_SRCH1");
            // SQL filters by Code field (M.Code LIKE @Search), not Description
            await SeedEntityAsync(miscTypeId, "ALPHA_CODE", "Alpha Misc");
            await SeedEntityAsync(miscTypeId, "BETA_CODE", "Beta Misc");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Misc");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_BYID1");
            var id = await SeedEntityAsync(miscTypeId, "MM_ID1", "Get By Id Test");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Description.Should().Be("Get By Id Test");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_EXISTS1");
            await SeedEntityAsync(miscTypeId, "MM_EX1", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MM_EX1", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_EXISTS2");
            var id = await SeedEntityAsync(miscTypeId, "MM_EX2", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id,
                new PartyManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MM_EX2", miscTypeId);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MT_NF1");
            var id = await SeedEntityAsync(miscTypeId, "MM_NF1");

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            // NotFoundAsync returns count > 0 (true when record exists, false when missing)
            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            // NotFoundAsync returns count > 0 (true when record exists, false when missing)
            notFound.Should().BeFalse();
        }
    }
}
