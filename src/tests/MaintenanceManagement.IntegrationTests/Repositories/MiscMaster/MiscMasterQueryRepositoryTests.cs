using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.MiscMaster
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

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int miscTypeId, string code = "MM_QRY001", string description = "Query Test")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT1");
            await SeedEntityAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT2");
            var id = await SeedEntityAsync(miscTypeId, "MM_DEL1", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMiscMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT3");
            await SeedEntityAsync(miscTypeId, "MM_ALPHA", "Alpha Item");
            await SeedEntityAsync(miscTypeId, "MM_BETA", "Beta Item");

            var (items, _) = await CreateQueryRepo().GetAllMiscMasterAsync(1, 10, "MM_ALPHA");

            items.Should().HaveCount(1);
            items[0].Code.Should().Be("MM_ALPHA");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT4");
            var id = await SeedEntityAsync(miscTypeId, "MM_ID1", "Get By Id Item");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("MM_ID1");
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
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT5");
            var id = await SeedEntityAsync(miscTypeId, "MM_DEL2", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT6");
            await SeedEntityAsync(miscTypeId, "MM_EX1", "Existing");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MM_EX1", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("MAINT_QT7");
            var id = await SeedEntityAsync(miscTypeId, "MM_EX2", "To Delete");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var exists = await CreateQueryRepo().AlreadyExistsAsync("MM_EX2", miscTypeId);

            exists.Should().BeFalse();
        }
    }
}
