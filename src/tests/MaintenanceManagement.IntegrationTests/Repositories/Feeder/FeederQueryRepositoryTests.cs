using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.Feeder
{
    [Collection("DatabaseCollection")]
    public sealed class FeederQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FeederQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FeederQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FeederQueryRepository(conn, _fixture.IpMock.Object);
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

        private async Task<int> SeedFeederTypeAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"FDRQ_MT_{suffix}",
                    Description = "FeederType",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var misc = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id,
                    Code = $"FDRQ_MM_{suffix}",
                    Description = $"Desc {suffix}",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return misc.Id;
        }

        private async Task<int> SeedFeederAsync(string code, string name, int fgId, int ftId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.Feeder
                {
                    FeederCode = code,
                    FeederName = name,
                    FeederGroupId = fgId,
                    FeederTypeId = ftId,
                    DepartmentId = 1,
                    Description = "Test feeder",
                    MultiplicationFactor = 1.0m,
                    EffectiveDate = DateTimeOffset.UtcNow,
                    OpeningReading = 0.0m,
                    HighPriority = false,
                    Target = 0.0m,
                    UnitId = 1,
                    MeterAvailable = true,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllFeederAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G1", "GQ1");
            var ftId = await SeedFeederTypeAsync("Q1");
            await SeedFeederAsync("FDR_Q1", "Query Feeder", fgId, ftId);

            var (items, total) = await CreateQueryRepo().GetAllFeederAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].FeederCode.Should().Be("FDR_Q1");
        }

        [Fact]
        public async Task GetAllFeederAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G2", "GQ2");
            var ftId = await SeedFeederTypeAsync("Q2");
            var id = await SeedFeederAsync("FDR_Q2", "Deleted Feeder", fgId, ftId);

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
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G3", "GQ3");
            var ftId = await SeedFeederTypeAsync("Q3");
            await SeedFeederAsync("FDR_ALPHA", "Alpha Feeder", fgId, ftId);
            await SeedFeederAsync("FDR_BETA", "Beta Feeder", fgId, ftId);

            var (items, _) = await CreateQueryRepo().GetAllFeederAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].FeederCode.Should().Be("FDR_ALPHA");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetFeederByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G4", "GQ4");
            var ftId = await SeedFeederTypeAsync("Q4");
            var id = await SeedFeederAsync("FDR_BY_ID", "ById Feeder", fgId, ftId);

            var result = await CreateQueryRepo().GetFeederByIdAsync(id);

            result.Should().NotBeNull();
            result!.FeederCode.Should().Be("FDR_BY_ID");
        }

        [Fact]
        public async Task GetFeederByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetFeederByIdAsync(99999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G5", "GQ5");
            var ftId = await SeedFeederTypeAsync("Q5");
            await SeedFeederAsync("FDR_DUP", "Dup Feeder", fgId, ftId);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("FDR_DUP");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOT_EXISTS");

            exists.Should().BeFalse();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G6", "GQ6");
            var ftId = await SeedFeederTypeAsync("Q6");
            var id = await SeedFeederAsync("FDR_SDV", "No Dep Feeder", fgId, ftId);

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        // --- IS LINKED ---

        [Fact]
        public async Task IsFeederLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var fgId = await SeedFeederGroupAsync("FGQ_G7", "GQ7");
            var ftId = await SeedFeederTypeAsync("Q7");
            var id = await SeedFeederAsync("FDR_UL", "Unlinked Feeder", fgId, ftId);

            var linked = await CreateQueryRepo().IsFeederLinkedAsync(id);

            linked.Should().BeFalse();
        }
    }
}
