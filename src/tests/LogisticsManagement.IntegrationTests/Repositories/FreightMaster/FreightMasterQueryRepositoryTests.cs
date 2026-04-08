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
    public sealed class FreightMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FreightMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FreightMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FreightMasterQueryRepository(conn);
        }

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

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code, string description)
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

        private async Task<int> SeedFreightAsync(int freightModeId, int rateMethodId, decimal rate = 100.50m, int moduleId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FreightMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.FreightMaster
                {
                    FreightModeId = freightModeId,
                    RateMethodId = rateMethodId,
                    Rate = rate,
                    ModuleId = moduleId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Logistics.FreightMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            await SeedFreightAsync(modeId, methodId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_JoinedFields()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            await SeedFreightAsync(modeId, methodId, 150.25m, 3);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].FreightModeId.Should().Be(modeId);
            items[0].FreightModeName.Should().Be("Road Transport");
            items[0].RateMethodId.Should().Be(methodId);
            items[0].RateMethodName.Should().Be("Per Kg");
            items[0].Rate.Should().Be(150.25m);
            items[0].ModuleId.Should().Be(3);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FreightMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var modeRoad = await SeedMiscMasterAsync(miscTypeId, "ROAD", "Road Transport");
            var modeRail = await SeedMiscMasterAsync(miscTypeId, "RAIL", "Rail Transport");
            var method = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");

            await SeedFreightAsync(modeRoad, method, 100m);
            await SeedFreightAsync(modeRail, method, 200m);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Road");

            items.Should().HaveCount(1);
            items[0].FreightModeName.Should().Be("Road Transport");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var method = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");

            for (int i = 1; i <= 5; i++)
            {
                var mode = await SeedMiscMasterAsync(miscTypeId, $"MODE{i:D3}", $"Mode {i}");
                await SeedFreightAsync(mode, method, i * 10m, i);
            }

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);

            total.Should().Be(5);
            page1.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_EmptyResult_ReturnsZeroCount()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId, 300.00m, 2);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.FreightModeId.Should().Be(modeId);
            dto.FreightModeName.Should().Be("Road Transport");
            dto.RateMethodId.Should().Be(methodId);
            dto.RateMethodName.Should().Be("Per Kg");
            dto.Rate.Should().Be(300.00m);
            dto.ModuleId.Should().Be(2);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FreightMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- COMPOSITE KEY EXISTS ---

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            await SeedFreightAsync(modeId, methodId, 100m, 1);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(modeId, methodId, 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(modeId, methodId, 1);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId, 100m, 1);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FreightMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(modeId, methodId, 1);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId, 100m, 1);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(modeId, methodId, 1, id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Different_ModuleId_Should_Return_False()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            await SeedFreightAsync(modeId, methodId, 100m, 1);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(modeId, methodId, 2);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FreightMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // --- MISC MASTER EXISTS ---

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var (modeId, _) = await SeedPrerequisitesAsync();

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(modeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Inactive()
        {
            await ClearTablesAsync();
            var (modeId, _) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscMaster.FirstAsync(x => x.Id == modeId);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(modeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, _) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(modeId, CancellationToken.None);

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(modeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(9999);

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var modeRoad = await SeedMiscMasterAsync(miscTypeId, "ROAD", "Road Transport");
            var modeRail = await SeedMiscMasterAsync(miscTypeId, "RAIL", "Rail Transport");
            var method = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");

            await SeedFreightAsync(modeRoad, method, 100m);
            await SeedFreightAsync(modeRail, method, 200m);

            var results = await CreateQueryRepo().AutocompleteAsync("Road", null, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].FreightModeName.Should().Be("Road Transport");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_ModuleId()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var modeRoad = await SeedMiscMasterAsync(miscTypeId, "ROAD", "Road Transport");
            var modeRail = await SeedMiscMasterAsync(miscTypeId, "RAIL", "Rail Transport");
            var method = await SeedMiscMasterAsync(miscTypeId, "PERKG", "Per Kg");

            await SeedFreightAsync(modeRoad, method, 100m, 1);
            await SeedFreightAsync(modeRail, method, 200m, 2);

            var results = await CreateQueryRepo().AutocompleteAsync("", 1, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].FreightModeName.Should().Be("Road Transport");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.FreightMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var (modeId, methodId) = await SeedPrerequisitesAsync();
            var id = await SeedFreightAsync(modeId, methodId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new FreightMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- IS VALID MODE METHOD COMBINATION ---

        [Fact]
        public async Task IsValidModeMethodCombinationAsync_Should_Return_False_When_Mode_NotExists()
        {
            await ClearTablesAsync();

            var valid = await CreateQueryRepo().IsValidModeMethodCombinationAsync(9999, 9998);

            valid.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidModeMethodCombinationAsync_Should_Return_False_When_Method_NotExists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var modeId = await SeedMiscMasterAsync(miscTypeId, "PERKM", "PER_KM");

            var valid = await CreateQueryRepo().IsValidModeMethodCombinationAsync(modeId, 9999);

            valid.Should().BeFalse();
        }
    }
}
