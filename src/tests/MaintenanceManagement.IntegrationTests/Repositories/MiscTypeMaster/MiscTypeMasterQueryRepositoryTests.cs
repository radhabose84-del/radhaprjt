using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.MiscTypeMaster
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

        private async Task<int> SeedEntityAsync(string code = "MAINT_QRY001", string description = "Query Test Type")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscTypeMaster]");
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
        public async Task GetAllMiscTypeMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MAINT_DEL1", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscTypeMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("MAINT_ALPHA", "Alpha Type");
            await SeedEntityAsync("MAINT_BETA", "Beta Type");

            var (items, _) = await CreateQueryRepo().GetAllMiscTypeMasterAsync(1, 10, "MAINT_ALPHA");

            items.Should().HaveCount(1);
            items[0].MiscTypeCode.Should().Be("MAINT_ALPHA");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MAINT_ID1", "Get By Id Type");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MiscTypeCode.Should().Be("MAINT_ID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MAINT_DEL2", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("MAINT_EX1", "Existing Type");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MAINT_EX1");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("MAINT_EX2", "To Delete Type");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MAINT_EX2");

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetMiscTypeMaster_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("MAINT_AC1", "Autocomplete Type");

            var results = await CreateQueryRepo().GetMiscTypeMaster("MAINT_AC1");

            results.Should().NotBeEmpty();
        }
    }
}
