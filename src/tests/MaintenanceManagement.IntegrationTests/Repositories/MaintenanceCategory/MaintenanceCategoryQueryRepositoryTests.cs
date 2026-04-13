using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceCategory;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceCategory
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceCategoryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceCategoryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceCategoryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceCategoryQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string name = "Query Test Category", string description = "Test Desc")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MaintenanceCategoryCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MaintenanceCategory
            {
                CategoryName = name,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMaintenanceCategoryAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMaintenanceCategoryAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMaintenanceCategoryAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("To Delete Category");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MaintenanceCategoryCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMaintenanceCategoryAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMaintenanceCategoryAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Alpha Category");
            await SeedEntityAsync("Beta Category");

            var (items, _) = await CreateQueryRepo().GetAllMaintenanceCategoryAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].CategoryName.Should().Be("Alpha Category");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Get By Id Category");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CategoryName.Should().Be("Get By Id Category");
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
            var id = await SeedEntityAsync("Soft Deleted Category");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MaintenanceCategoryCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MaintenanceCategory { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
