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

        public FeederGroupQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private FeederGroupQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fixture.IpMock.Object);

        private async Task<int> SeedEntityAsync(string code = "FGQ001", string name = "Query Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new FeederGroupCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.Power.FeederGroup
            {
                FeederGroupCode = code,
                FeederGroupName = name,
                UnitId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[PowerConsumption]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[Feeder]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[FeederGroup]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FGQ_DEL", "Delete Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FeederGroupCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ALPHA001", "Alpha FeederGroup");
            await SeedEntityAsync("BETA001", "Beta FeederGroup");

            var (items, _) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].FeederGroupName.Should().Be("Alpha FeederGroup");
        }

        [Fact]
        public async Task GetAllFeederGroupAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("FGP001", "Group One");
            await SeedEntityAsync("FGP002", "Group Two");
            await SeedEntityAsync("FGP003", "Group Three");

            var (items, total) = await CreateQueryRepo().GetAllFeederGroupAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetFeederGroupByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FGG001", "GetById Group");

            var result = await CreateQueryRepo().GetFeederGroupByIdAsync(id);

            result.Should().NotBeNull();
            result!.FeederGroupName.Should().Be("GetById Group");
            result.FeederGroupCode.Should().Be("FGG001");
        }

        [Fact]
        public async Task GetFeederGroupByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetFeederGroupByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFeederGroupByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FGS001", "SoftDeleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FeederGroupCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetFeederGroupByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("FGE001", "Exists Group");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("FGE001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_New_Code()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NEWCODE999");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FGNF001", "NotFound Group");

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTableAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FGSD001", "No Dependent Group");

            var result = await CreateQueryRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
