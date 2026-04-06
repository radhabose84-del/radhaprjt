using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.WorkCenter;

namespace MaintenanceManagement.IntegrationTests.Repositories.WorkCenter
{
    [Collection("DatabaseCollection")]
    public sealed class WorkCenterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkCenterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkCenterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WorkCenterQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedEntityAsync(
            string code = "WC_QRY001",
            string name = "Query Work Center",
            int unitId = 1,
            int departmentId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new WorkCenterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                WorkCenterCode = code,
                WorkCenterName = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[WorkCenter]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllWorkCenterGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllWorkCenterGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllWorkCenterGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("WC_DEL1", "To Delete WC");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new WorkCenterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.WorkCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllWorkCenterGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllWorkCenterGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("WC_ALPHA", "Alpha Work Center");
            await SeedEntityAsync("WC_BETA", "Beta Work Center");

            var (items, _) = await CreateQueryRepo().GetAllWorkCenterGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].WorkCenterName.Should().Be("Alpha Work Center");
        }

        [Fact]
        public async Task GetAllWorkCenterGroupAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("WC_PG1", "Work Center 1");
            await SeedEntityAsync("WC_PG2", "Work Center 2");
            await SeedEntityAsync("WC_PG3", "Work Center 3");

            var (items, total) = await CreateQueryRepo().GetAllWorkCenterGroupAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("WC_ID1", "GetById Work Center");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.WorkCenterCode.Should().Be("WC_ID1");
            result.WorkCenterName.Should().Be("GetById Work Center");
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
            var id = await SeedEntityAsync("WC_SDEL", "Soft Deleted WC");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new WorkCenterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.WorkCenter { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetWorkCenterGroups_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("WC_AUTO1", "Autocomplete Work Center");

            var results = await CreateQueryRepo().GetWorkCenterGroups("Autocomplete");

            results.Should().HaveCount(1);
            results[0].WorkCenterName.Should().Be("Autocomplete Work Center");
        }

        [Fact]
        public async Task GetWorkCenterGroups_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("WC_INACT", "Inactive Work Center");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.WorkCenter.FindAsync(id);
            entity!.IsActive = BaseEntity.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().GetWorkCenterGroups("Inactive");

            results.Should().BeEmpty();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("WC_NF1", "Exists WC");

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
            var id = await SeedEntityAsync("WC_NOLINK", "No Links WC");

            var result = await CreateQueryRepo().SoftDeleteValidation(id);

            result.Should().BeFalse();
        }
    }
}
