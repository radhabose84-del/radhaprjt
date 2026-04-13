using Contracts.Interfaces;
using Dapper;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveSchedulerLog;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Repositories.PreventiveSchedulers;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace MaintenanceManagement.IntegrationTests.Repositories.PreventiveSchedulers
{
    [Collection("DatabaseCollection")]
    public sealed class PreventiveSchedulerCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PreventiveSchedulerCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PreventiveSchedulerCommandRepository CreateRepository(
            MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var schedulerQueryMock = new Mock<IPreventiveSchedulerQuery>(MockBehavior.Loose);
            var miscMasterQueryMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            var ipMock = _fixture.IpMock;
            var logServiceMock = new Mock<IPreventiveScheduleLogService>(MockBehavior.Loose);
            var httpContextMock = new Mock<IHttpContextAccessor>(MockBehavior.Loose);

            return new PreventiveSchedulerCommandRepository(
                ctx,
                schedulerQueryMock.Object,
                miscMasterQueryMock.Object,
                ipMock.Object,
                logServiceMock.Object,
                httpContextMock.Object,
                NullLogger<PreventiveSchedulerCommandRepository>.Instance);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteAsync(
                999999,
                new PreventiveSchedulerHeader
                {
                    MachineGroup = null!,
                    MiscMaintenanceCategory = null!,
                    MiscSchedule = null!,
                    MiscFrequencyType = null!,
                    MiscFrequencyUnit = null!
                });

            result.Should().BeFalse();
        }

        // --- UpdateDetailAsync ---

        [Fact]
        public async Task UpdateDetailAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateDetailAsync(999999, "NEW_JOB_ID");

            result.Should().BeFalse();
        }

        // --- UpdateRescheduleDate ---

        [Fact]
        public async Task UpdateRescheduleDate_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateRescheduleDate(
                999999,
                DateOnly.FromDateTime(DateTime.Today));

            result.Should().BeFalse();
        }

        // --- DeleteDetailAsync ---

        [Fact]
        public async Task DeleteDetailAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteDetailAsync(999999);

            result.Should().BeFalse();
        }

        // --- DeleteDetailByDetailId ---

        [Fact]
        public async Task DeleteDetailByDetailId_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteDetailByDetailId(999999);

            result.Should().BeFalse();
        }

        // --- BulkImport with empty list ---

        [Fact]
        public async Task BulkImportPreventiveHeaderAsync_Should_Return_EmptyList_When_Empty_Input()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx)
                .BulkImportPreventiveHeaderAsync(new List<PreventiveSchedulerHeader>());

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetDetailByMachineActivityAndUnitAsync ---

        [Fact]
        public async Task GetDetailByMachineActivityAndUnitAsync_Should_Return_Null_When_Empty()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx)
                .GetDetailByMachineActivityAndUnitAsync("NONEXISTENT", "NONEXISTENT_ACT", 1);

            result.Should().BeNull();
        }
    }
}
