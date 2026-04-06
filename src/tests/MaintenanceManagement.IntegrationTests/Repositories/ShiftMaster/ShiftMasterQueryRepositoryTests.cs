using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.ShiftMaster;

namespace MaintenanceManagement.IntegrationTests.Repositories.ShiftMaster
{
    [Collection("DatabaseCollection")]
    public sealed class ShiftMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ShiftMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ShiftMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ShiftMasterQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(
            string code = "SHF_QRY001",
            string name = "Query Shift")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ShiftMasterCommandRepository(ctx);
            return await repo.CreateAsync(new MaintenanceManagement.Domain.Entities.ShiftMaster
            {
                ShiftCode = code,
                ShiftName = name,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
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
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMasterDetails]");
            await conn.ExecuteAsync("DELETE FROM [Maintenance].[ShiftMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllShiftMasterAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllShiftMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllShiftMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("SHF_DEL1", "To Delete Shift");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ShiftMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllShiftMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllShiftMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("SHF_ALPHA", "Alpha Shift");
            await SeedEntityAsync("SHF_BETA", "Beta Shift");

            var (items, _) = await CreateQueryRepo().GetAllShiftMasterAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].ShiftName.Should().Be("Alpha Shift");
        }

        [Fact]
        public async Task GetAllShiftMasterAsync_Should_Respect_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("SHF_PG1", "Shift Page 1");
            await SeedEntityAsync("SHF_PG2", "Shift Page 2");
            await SeedEntityAsync("SHF_PG3", "Shift Page 3");

            var (items, total) = await CreateQueryRepo().GetAllShiftMasterAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("SHF_ID1", "GetById Shift");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.ShiftCode.Should().Be("SHF_ID1");
            result.ShiftName.Should().Be("GetById Shift");
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
            var id = await SeedEntityAsync("SHF_SDEL", "Soft Deleted Shift");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ShiftMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate_Name()
        {
            await ClearTableAsync();
            await SeedEntityAsync("SHF_EX1", "Existing Shift");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Existing Shift");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("SHF_EX2", "Deleted Shift Name");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new ShiftMasterCommandRepository(ctx).DeleteAsync(id,
                new MaintenanceManagement.Domain.Entities.ShiftMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Deleted Shift Name");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsShiftCodeAsync_Should_Return_True_When_Duplicate_Code()
        {
            await ClearTableAsync();
            await SeedEntityAsync("SHF_CODE1", "Code Shift");

            var exists = await CreateQueryRepo().AlreadyExistsShiftCodeAsync("SHF_CODE1");

            exists.Should().BeTrue();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("SHF_NF1", "Exists Shift");

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

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetShiftMaster_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("SHF_AUTO1", "Autocomplete Shift");

            var results = await CreateQueryRepo().GetShiftMaster("Autocomplete");

            results.Should().HaveCount(1);
            results[0].ShiftName.Should().Be("Autocomplete Shift");
        }
    }
}
