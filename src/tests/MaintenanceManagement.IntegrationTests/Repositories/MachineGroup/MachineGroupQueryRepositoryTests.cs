using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineGroup
{
    [Collection("DatabaseCollection")]
    public sealed class MachineGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MachineGroupQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedEntityAsync(
            string groupName = "Query Group",
            int unitId = 1,
            int departmentId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MachineGroupCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = groupName,
                Manufacturer = 1,
                UnitId = unitId,
                DepartmentId = departmentId,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMachineGroupsAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupsAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMachineGroupsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("To Delete Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineGroupCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupsAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMachineGroupsAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Alpha Machine Group");
            await SeedEntityAsync("Beta Machine Group");

            var (items, _) = await CreateQueryRepo().GetAllMachineGroupsAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].GroupName.Should().Be("Alpha Machine Group");
        }

        [Fact]
        public async Task GetAllMachineGroupsAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Group One");
            await SeedEntityAsync("Group Two");
            await SeedEntityAsync("Group Three");

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupsAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("GetById Group");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.GroupName.Should().Be("GetById Group");
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
            var id = await SeedEntityAsync("Soft Deleted Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineGroupCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetMachineGroupAutoComplete_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Autocomplete Group");

            var results = await CreateQueryRepo().GetMachineGroupAutoComplete("Autocomplete");

            results.Should().HaveCount(1);
            results[0].GroupName.Should().Be("Autocomplete Group");
        }

        [Fact]
        public async Task GetMachineGroupAutoComplete_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Inactive Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MachineGroup.FindAsync(id);
            entity!.IsActive = BaseEntity.Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().GetMachineGroupAutoComplete("Inactive");

            results.Should().BeEmpty();
        }

        // --- EXISTS / NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Exists Group");

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

        [Fact]
        public async Task FKColumnExistValidation_Should_Return_True_For_Active_Group()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("FK Valid Group");

            var exists = await CreateQueryRepo().FKColumnExistValidation(id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("No Dependent Group");

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
