using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceType;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceType
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceTypeQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceTypeQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceTypeQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string typeName = "Corrective")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MaintenanceTypeCommandRepository(ctx);
            var entity = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                TypeName = typeName,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            return await repo.CreateAsync(entity);
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MaintenanceType]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMaintenanceTypeAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMaintenanceTypeAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMaintenanceTypeAsync_Should_Return_Correct_TypeName()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Preventive");

            var (items, _) = await CreateQueryRepo().GetAllMaintenanceTypeAsync(1, 10, null);

            items[0].TypeName.Should().Be("Preventive");
        }

        [Fact]
        public async Task GetAllMaintenanceTypeAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new MaintenanceTypeCommandRepository(ctx).DeleteAsync(id, toDelete);

            var (items, total) = await CreateQueryRepo().GetAllMaintenanceTypeAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMaintenanceTypeAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Corrective");
            await SeedEntityAsync("Preventive");

            var (items, _) = await CreateQueryRepo().GetAllMaintenanceTypeAsync(1, 10, "Corr");

            items.Should().HaveCount(1);
            items[0].TypeName.Should().Be("Corrective");
        }

        [Fact]
        public async Task GetAllMaintenanceTypeAsync_Should_Return_Multiple_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Corrective");
            await SeedEntityAsync("Preventive");
            await SeedEntityAsync("Predictive");

            var (items, total) = await CreateQueryRepo().GetAllMaintenanceTypeAsync(1, 10, null);

            items.Should().HaveCount(3);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Corrective");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.TypeName.Should().Be("Corrective");
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
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var toDelete = new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };
            await new MaintenanceTypeCommandRepository(ctx).DeleteAsync(id, toDelete);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- GET AUTOCOMPLETE ---

        [Fact]
        public async Task GetMaintenanceTypeAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Corrective");
            await SeedEntityAsync("Conditional");
            await SeedEntityAsync("Preventive");

            var results = await CreateQueryRepo().GetMaintenanceTypeAsync("C");

            results.Should().HaveCount(2);
        }
    }
}
