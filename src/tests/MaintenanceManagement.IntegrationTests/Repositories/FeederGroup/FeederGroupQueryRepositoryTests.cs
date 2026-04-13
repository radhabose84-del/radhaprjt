using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.FeederGroup
{
    [Collection("DatabaseCollection")]
    public sealed class FeederGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FeederGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FeederGroupQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedFeederGroupAsync(string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = code,
                    FeederGroupName = name,
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedFeederGroupAsync("FGQ_G1", "Group Q1");

            var (items, total) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, null!);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].FeederGroupCode.Should().Be("FGQ_G1");
        }

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedFeederGroupAsync("FGQ_G2", "Group Q2");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FeederGroupCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, null!);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedFeederGroupAsync("FGQ_ALPHA", "Alpha");
            await SeedFeederGroupAsync("FGQ_BETA", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].FeederGroupCode.Should().Be("FGQ_ALPHA");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetFeederGroupByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var id = await SeedFeederGroupAsync("FGQ_ID1", "ByIdGroup");

            var result = await CreateQueryRepo().GetFeederGroupByIdAsync(id);

            result.Should().NotBeNull();
            result!.FeederGroupCode.Should().Be("FGQ_ID1");
        }

        [Fact]
        public async Task GetFeederGroupByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetFeederGroupByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            await SeedFeederGroupAsync("FGQ_DUP", "DupGroup");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("FGQ_DUP");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("FGQ_NOT_EXISTS");

            exists.Should().BeFalse();
        }

        // --- IS LINKED ---

        [Fact]
        public async Task IsFeederGroupLinkedAsync_Should_Return_False_When_No_Feeders()
        {
            await ClearTablesAsync();
            var id = await SeedFeederGroupAsync("FGQ_UL", "Unlinked");

            var linked = await CreateQueryRepo().IsFeederGroupLinkedAsync(id);

            linked.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Feeders()
        {
            await ClearTablesAsync();
            var id = await SeedFeederGroupAsync("FGQ_SDV", "SdvGroup");

            var result = await CreateQueryRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
