using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Repositories.MiscMaster;
using MaintenanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using MaintenanceManagement.Infrastructure.Repositories.Power.Feeder;
using MaintenanceManagement.Infrastructure.Repositories.Power.FeederGroup;
using MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption;

namespace MaintenanceManagement.IntegrationTests.Repositories.PowerConsumption
{
    [Collection("DatabaseCollection")]
    public sealed class PowerConsumptionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PowerConsumptionQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PowerConsumptionQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PowerConsumptionQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task<int> SeedMiscMasterAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscTypeResult = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = $"PCQ_MT_{suffix}",
                    Description = "PCQ Test Type",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            var miscResult = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeResult.Id,
                    Code = $"PCQ_MM_{suffix}",
                    Description = $"FType {suffix}",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
            return miscResult.Id;
        }

        private async Task<int> SeedFeederGroupAsync(string suffix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederGroupCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.FeederGroup
                {
                    FeederGroupCode = $"PCQ_FG_{suffix}",
                    FeederGroupName = $"FG {suffix}",
                    UnitId = 1,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedFeederAsync(string suffix, int feederGroupId, int feederTypeId, decimal openingReading = 0m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new FeederCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.Feeder
                {
                    FeederCode = $"PCQ_F_{suffix}",
                    FeederName = $"Feeder {suffix}",
                    FeederGroupId = feederGroupId,
                    FeederTypeId = feederTypeId,
                    DepartmentId = 1,
                    Description = "Desc",
                    MultiplicationFactor = 1.0m,
                    EffectiveDate = DateTimeOffset.UtcNow,
                    OpeningReading = openingReading,
                    HighPriority = false,
                    Target = 0m,
                    UnitId = 1,
                    MeterAvailable = false,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedPowerConsumptionAsync(int feederTypeId, int feederId, decimal opening = 100m, decimal closing = 200m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new PowerConsumptionCommandRepository(ctx).CreateAsync(
                new MaintenanceManagement.Domain.Entities.Power.PowerConsumption
                {
                    FeederTypeId = feederTypeId,
                    FeederId = feederId,
                    UnitId = 1,
                    OpeningReading = opening,
                    ClosingReading = closing,
                    TotalUnits = closing - opening,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllPowerConsumptionAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var feederTypeId = await SeedMiscMasterAsync("Q1");
            var feederGroupId = await SeedFeederGroupAsync("Q1");
            var feederId = await SeedFeederAsync("Q1", feederGroupId, feederTypeId);
            await SeedPowerConsumptionAsync(feederTypeId, feederId);

            var (items, total) = await CreateQueryRepo().GetAllPowerConsumptionAsync(1, 10, null!);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].FeederId.Should().Be(feederId);
            items[0].FeederTypeId.Should().Be(feederTypeId);
        }

        [Fact]
        public async Task GetAllPowerConsumptionAsync_Should_Return_Empty_When_No_Records()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllPowerConsumptionAsync(1, 10, null!);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetPowerConsumptionById_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var feederTypeId = await SeedMiscMasterAsync("Q2");
            var feederGroupId = await SeedFeederGroupAsync("Q2");
            var feederId = await SeedFeederAsync("Q2", feederGroupId, feederTypeId);
            var id = await SeedPowerConsumptionAsync(feederTypeId, feederId, opening: 10m, closing: 50m);

            var result = await CreateQueryRepo().GetPowerConsumptionById(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.FeederId.Should().Be(feederId);
            result.OpeningReading.Should().Be(10m);
            result.ClosingReading.Should().Be(50m);
        }

        [Fact]
        public async Task GetPowerConsumptionById_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetPowerConsumptionById(9999);

            result.Should().BeNull();
        }

        // --- GET FEEDER SUB FEEDERS ---

        [Fact]
        public async Task GetFeederSubFeedersById_Should_Return_Feeders_For_Type()
        {
            await ClearTablesAsync();
            var feederTypeId = await SeedMiscMasterAsync("Q3");
            var feederGroupId = await SeedFeederGroupAsync("Q3");
            await SeedFeederAsync("Q3A", feederGroupId, feederTypeId);
            await SeedFeederAsync("Q3B", feederGroupId, feederTypeId);

            var result = await CreateQueryRepo().GetFeederSubFeedersById(feederTypeId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetFeederSubFeedersById_Should_Return_Empty_For_Unknown_Type()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetFeederSubFeedersById(9999);

            result.Should().BeEmpty();
        }

        // --- GET OPENING READER VALUE ---

        [Fact]
        public async Task GetOpeningReaderValueById_Should_Return_Feeder_Opening_When_No_Consumption()
        {
            await ClearTablesAsync();
            var feederTypeId = await SeedMiscMasterAsync("Q4");
            var feederGroupId = await SeedFeederGroupAsync("Q4");
            var feederId = await SeedFeederAsync("Q4", feederGroupId, feederTypeId, openingReading: 42m);

            var result = await CreateQueryRepo().GetOpeningReaderValueById(feederId);

            result.Should().NotBeNull();
            result.FeederId.Should().Be(feederId);
            result.OpeningReading.Should().Be(42m);
        }

        [Fact]
        public async Task GetOpeningReaderValueById_Should_Return_Latest_ClosingReading_From_Consumption()
        {
            await ClearTablesAsync();
            var feederTypeId = await SeedMiscMasterAsync("Q5");
            var feederGroupId = await SeedFeederGroupAsync("Q5");
            var feederId = await SeedFeederAsync("Q5", feederGroupId, feederTypeId, openingReading: 0m);
            await SeedPowerConsumptionAsync(feederTypeId, feederId, opening: 0m, closing: 123m);

            var result = await CreateQueryRepo().GetOpeningReaderValueById(feederId);

            result.Should().NotBeNull();
            result.OpeningReading.Should().Be(123m);
        }

        [Fact]
        public async Task GetOpeningReaderValueById_Should_Throw_When_Feeder_Not_Found()
        {
            await ClearTablesAsync();

            Func<Task> act = async () => await CreateQueryRepo().GetOpeningReaderValueById(9999);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*not found*");
        }
    }
}
