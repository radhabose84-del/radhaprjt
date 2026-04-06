using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMasterDetailRepo;

namespace MaintenanceManagement.IntegrationTests.Repositories.ShiftMasterDetail
{
    [Collection("DatabaseCollection")]
    public sealed class ShiftMasterDetailQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ShiftMasterDetailQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ShiftMasterDetailQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ShiftMasterDetailQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedShiftMasterAsync(string code = "SMD_QRY_SM1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ShiftMasterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                ShiftCode = code,
                ShiftName = $"Shift {code}",
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityAsync(int shiftMasterId, int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ShiftMasterDetailCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.ShiftMasterDetail
            {
                ShiftMasterId = shiftMasterId,
                UnitId = unitId,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(16, 0),
                DurationInHours = 8,
                BreakDurationInMinutes = 30,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                ShiftSupervisorId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineSpecification]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[MachineMaster]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMasterDetails]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllShiftMasterDetailAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S1");
            await SeedEntityAsync(shiftMasterId);

            var (items, total) = await CreateQueryRepo().GetAllShiftMasterDetailAsync(1, 10, null);

            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllShiftMasterDetailAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S2");
            var id = await SeedEntityAsync(shiftMasterId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ShiftMasterDetailCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllShiftMasterDetailAsync(1, 10, null);

            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S3");
            var id = await SeedEntityAsync(shiftMasterId);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ShiftMasterId.Should().Be(shiftMasterId);
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
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S4");
            var id = await SeedEntityAsync(shiftMasterId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ShiftMasterDetailCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ShiftMasterDetail { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Detail_Exists_For_ShiftMaster()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S5");
            await SeedEntityAsync(shiftMasterId);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(shiftMasterId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_No_Details()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S6");

            var exists = await CreateQueryRepo().AlreadyExistsAsync(shiftMasterId);

            exists.Should().BeFalse();
        }

        // --- FK COLUMN VALIDATION ---

        [Fact]
        public async Task FKColumnValidation_Should_Return_True_When_ShiftMaster_Exists()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S7");

            var result = await CreateQueryRepo().FKColumnValidation(shiftMasterId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task FKColumnValidation_Should_Return_False_When_ShiftMaster_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().FKColumnValidation(9999);

            result.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var shiftMasterId = await SeedShiftMasterAsync("SMD_QRY_S8");
            var id = await SeedEntityAsync(shiftMasterId);

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
    }
}
