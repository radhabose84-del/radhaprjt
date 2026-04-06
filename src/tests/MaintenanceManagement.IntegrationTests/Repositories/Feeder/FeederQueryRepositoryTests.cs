using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.Feeder
{
    [Collection("DatabaseCollection")]
    public sealed class FeederQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private FeederQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), _fixture.IpMock.Object);

        private async Task SeedFeederGroupIfNeededAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            if (!await ctx.FeederGroup.AnyAsync(x => x.Id == 1))
            {
                await ctx.FeederGroup.AddAsync(new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = "FGQ_BASE",
                    FeederGroupName = "Base Feeder Group",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }
        }

        private async Task<int> SeedEntityAsync(string code = "FDRQ001", string name = "Query Feeder")
        {
            await SeedFeederGroupIfNeededAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var feederGroupId = ctx.FeederGroup.First().Id;
            var repo = new FeederCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.Power.Feeder
            {
                FeederCode = code,
                FeederName = name,
                UnitId = 1,
                FeederGroupId = feederGroupId,
                FeederTypeId = 1,
                DepartmentId = 1,
                EffectiveDate = DateTimeOffset.UtcNow,
                MeterAvailable = false,
                HighPriority = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
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
        public async Task GetAllFeederAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllFeederAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllFeederAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FDR_DEL", "Delete Feeder");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FeederCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllFeederAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllFeederAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("ALPH001", "Alpha Feeder");
            await SeedEntityAsync("BETA001", "Beta Feeder");

            var (items, _) = await CreateQueryRepo().GetAllFeederAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].FeederName.Should().Be("Alpha Feeder");
        }

        [Fact]
        public async Task GetAllFeederAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("FDR_P001", "Feeder One");
            await SeedEntityAsync("FDR_P002", "Feeder Two");
            await SeedEntityAsync("FDR_P003", "Feeder Three");

            var (items, total) = await CreateQueryRepo().GetAllFeederAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetFeederByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FDR_G001", "GetById Feeder");

            var result = await CreateQueryRepo().GetFeederByIdAsync(id);

            result.Should().NotBeNull();
            result!.FeederName.Should().Be("GetById Feeder");
            result.FeederCode.Should().Be("FDR_G001");
        }

        [Fact]
        public async Task GetFeederByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetFeederByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetFeederByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FDR_SD01", "SoftDeleted Feeder");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FeederCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.Power.Feeder { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetFeederByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("FDR_EX01", "Exists Feeder");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("FDR_EX01");

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
            var id = await SeedEntityAsync("FDR_NF01", "NotFound Feeder");

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
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FDR_VL01", "Validate Feeder");

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetFeederAutoComplete_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("FDR_AC01", "Autocomplete Feeder");

            var results = await CreateQueryRepo().GetFeederAutoComplete("Autocomplete");

            results.Should().HaveCount(1);
            results[0].FeederName.Should().Be("Autocomplete Feeder");
        }
    }
}
