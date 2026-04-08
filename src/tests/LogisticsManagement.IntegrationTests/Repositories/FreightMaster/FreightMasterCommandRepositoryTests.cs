using Dapper;
using Microsoft.Data.SqlClient;
using LogisticsManagement.Domain.Common;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Repositories.FreightMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.FreightMaster
{
    [Collection("DatabaseCollection")]
    public sealed class FreightMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FreightMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static FreightMasterCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeAsync(string code = "FMODE", string description = "Freight Mode")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "MM001", string description = "Test Misc")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = description,
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        /// <summary>
        /// Seeds the full prerequisite chain: MiscTypeMaster -> 2 MiscMaster records (mode + method).
        /// Returns (freightModeId, rateMethodId).
        /// </summary>
        private async Task<(int freightModeId, int rateMethodId)> SeedPrerequisitesAsync()
        {
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var freightModeId = await SeedMiscMasterAsync(miscTypeId, "ROAD", "Road Transport");
            var rateMethodId = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");
            return (freightModeId, rateMethodId);
        }

        private static Domain.Entities.FreightMaster BuildEntity(
            int freightModeId,
            int rateMethodId,
            decimal rate = 100.50m,
            int moduleId = 1) =>
            new()
            {
                FreightModeId = freightModeId,
                RateMethodId = rateMethodId,
                Rate = rate,
                ModuleId = moduleId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Logistics.FreightMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId, 250.75m, 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FreightMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.FreightModeId.Should().Be(modeId);
            saved.RateMethodId.Should().Be(methodId);
            saved.Rate.Should().Be(250.75m);
            saved.ModuleId.Should().Be(2);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FreightMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId, 100.00m));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.FreightMaster.FirstAsync(x => x.Id == id);
            entity.Rate = 999.99m;
            entity.ModuleId = 5;
            var result = await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.FreightMaster.FirstAsync(x => x.Id == id);
            updated.Rate.Should().Be(999.99m);
            updated.ModuleId.Should().Be(5);
            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(modeId, methodId);
            entity.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_Modified_Audit_Fields()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.FreightMaster.FirstAsync(x => x.Id == id);
            entity.Rate = 500.00m;
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.FreightMaster.FirstAsync(x => x.Id == id);
            updated.ModifiedBy.Should().Be(1);
            updated.ModifiedByName.Should().Be("test-user");
            updated.ModifiedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Can_Change_FreightModeId_And_RateMethodId()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var modeId1 = await SeedMiscMasterAsync(miscTypeId, "ROAD", "Road");
            var modeId2 = await SeedMiscMasterAsync(miscTypeId, "RAIL", "Rail");
            var methodId = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId1, methodId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var entity = await ctx2.FreightMaster.FirstAsync(x => x.Id == id);
            entity.FreightModeId = modeId2;
            await CreateRepository(ctx2).UpdateAsync(entity);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.FreightMaster.FirstAsync(x => x.Id == id);
            updated.FreightModeId.Should().Be(modeId2);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);
            ctx2.ChangeTracker.Clear();

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var deleted = await ctx3.FreightMaster.FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_Deleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(modeId, methodId));
            ctx.ChangeTracker.Clear();

            await using var ctx2 = _fixture.CreateFreshDbContext();
            await CreateRepository(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var result = await CreateRepository(ctx3).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
