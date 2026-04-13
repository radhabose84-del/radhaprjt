using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroup;
using MaintenanceManagement.Infrastructure.Repositories.MachineGroupUser;

namespace MaintenanceManagement.IntegrationTests.Repositories.MachineGroupUser
{
    [Collection("DatabaseCollection")]
    public sealed class MachineGroupUserQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MachineGroupUserQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MachineGroupUserQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MachineGroupUserQueryRepository(conn);
        }

        private async Task<int> SeedMachineGroupAsync(string groupName = "MGU_QRY_Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MachineGroupCommandRepository(ctx);
            var result = await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                GroupName = groupName,
                Manufacturer = 1,
                UnitId = 1,
                DepartmentId = 1,
                PowerSource = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int machineGroupId, int departmentId = 1, int userId = 100)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MachineGroupUserCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.MachineGroupUser
            {
                MachineGroupId = machineGroupId,
                DepartmentId = departmentId,
                UserId = userId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllMachineGroupUserAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G1");
            await SeedEntityAsync(machineGroupId);

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupUserAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllMachineGroupUserAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G2");
            var id = await SeedEntityAsync(machineGroupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineGroupUserCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineGroupUser { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupUserAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMachineGroupUserAsync_Should_Respect_Pagination()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G3");
            await SeedEntityAsync(machineGroupId, 1, 101);
            await SeedEntityAsync(machineGroupId, 1, 102);
            await SeedEntityAsync(machineGroupId, 1, 103);

            var (items, total) = await CreateQueryRepo().GetAllMachineGroupUserAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G4");
            var id = await SeedEntityAsync(machineGroupId, 2, 200);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MachineGroupId.Should().Be(machineGroupId);
            result.UserId.Should().Be(200);
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
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G5");
            var id = await SeedEntityAsync(machineGroupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MachineGroupUserCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.MachineGroupUser { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("MGU_QRY_G6");
            var id = await SeedEntityAsync(machineGroupId);

            var found = await CreateQueryRepo().NotFoundAsync(id);

            found.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var found = await CreateQueryRepo().NotFoundAsync(9999);

            found.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetMachineGroupUserByName_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var machineGroupId = await SeedMachineGroupAsync("AutoMGU Group");
            await SeedEntityAsync(machineGroupId);

            var results = await CreateQueryRepo().GetMachineGroupUserByName("AutoMGU");

            results.Should().HaveCount(1);
        }
    }
}
