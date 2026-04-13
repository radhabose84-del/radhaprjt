using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.CostCenter;
using Contracts.Interfaces;

namespace MaintenanceManagement.IntegrationTests.Repositories.CostCenter
{
    [Collection("DatabaseCollection")]
    public sealed class CostCenterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CostCenterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CostCenterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CostCenterQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedEntityAsync(
            string code = "CC_QRY001",
            string name = "Query Cost Center",
            int unitId = 1,
            int departmentId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CostCenterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.CostCenter
            {
                CostCenterCode = code,
                CostCenterName = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                EffectiveDate = DateTimeOffset.UtcNow,
                ResponsiblePerson = "Test Person",
                BudgetAllocated = 5000,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllCostCenterListGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllCostCenterListGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllCostCenterListGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("CC_DEL1", "To Delete CC");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CostCenterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.CostCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllCostCenterListGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllCostCenterListGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("CC_ALPHA", "Alpha Center");
            await SeedEntityAsync("CC_BETA", "Beta Center");

            var (items, _) = await CreateQueryRepo().GetAllCostCenterListGroupAsync(1, 10, "CC_ALPHA");

            items.Should().HaveCount(1);
            items[0].CostCenterCode.Should().Be("CC_ALPHA");
        }

        [Fact]
        public async Task GetAllCostCenterListGroupAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("CC_PG1", "Paging Center 1");
            await SeedEntityAsync("CC_PG2", "Paging Center 2");
            await SeedEntityAsync("CC_PG3", "Paging Center 3");

            var (items, total) = await CreateQueryRepo().GetAllCostCenterListGroupAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("CC_ID1", "GetById Center");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CostCenterCode.Should().Be("CC_ID1");
            result.CostCenterName.Should().Be("GetById Center");
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
            var id = await SeedEntityAsync("CC_SDEL", "Soft Deleted CC");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CostCenterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.CostCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetCostCenterGroups_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("CC_AUTO1", "Autocomplete Center");

            var results = await CreateQueryRepo().GetCostCenterGroups("Autocomplete");

            results.Should().HaveCount(1);
            results[0].CostCenterName.Should().Be("Autocomplete Center");
        }

        [Fact]
        public async Task GetCostCenterGroups_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("CC_INACT", "Inactive Center");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.CostCenter.FindAsync(id);
            entity!.IsActive = BaseEntity.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().GetCostCenterGroups("Inactive");

            results.Should().BeEmpty();
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("CC_NOLINK", "No Links CC");

            var result = await CreateQueryRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
