using Dapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestCommandRepository CreateRepository(
            MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx,
            Mock<IMaintenanceRequestQueryRepository> queryRepoMock = null,
            Mock<IWorkOrderCommandRepository> workOrderRepoMock = null)
        {
            queryRepoMock ??= new Mock<IMaintenanceRequestQueryRepository>(MockBehavior.Loose);
            workOrderRepoMock ??= new Mock<IWorkOrderCommandRepository>(MockBehavior.Loose);

            return new MaintenanceRequestCommandRepository(
                ctx,
                queryRepoMock.Object,
                workOrderRepoMock.Object,
                _fixture.IpMock.Object,
                _fixture.TzMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(
                new MaintenanceManagement.Domain.Entities.MaintenanceRequest
                {
                    Id = 999999,
                    RequestTypeId = 1,
                    MaintenanceTypeId = 1,
                    MachineId = 1,
                    CompanyId = 1,
                    UnitId = 1,
                    SourceId = 1,
                    MaintenanceDepartmentId = 1,
                    ProductionDepartmentId = 1
                });

            result.Should().BeFalse();
        }

        // --- UpdateStatusAsync ---

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_False_When_WO_Open()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var queryRepoMock = new Mock<IMaintenanceRequestQueryRepository>(MockBehavior.Loose);
            queryRepoMock.Setup(x => x.GetWOclosedOrInProgressAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            var result = await CreateRepository(ctx, queryRepoMock).UpdateStatusAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_False_When_Status_List_Empty()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var queryRepoMock = new Mock<IMaintenanceRequestQueryRepository>(MockBehavior.Loose);
            queryRepoMock.Setup(x => x.GetWOclosedOrInProgressAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            queryRepoMock.Setup(x => x.GetMaintenancestatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());

            var result = await CreateRepository(ctx, queryRepoMock).UpdateStatusAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateStatusAsync_Should_Return_False_When_Request_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var queryRepoMock = new Mock<IMaintenanceRequestQueryRepository>(MockBehavior.Loose);
            queryRepoMock.Setup(x => x.GetWOclosedOrInProgressAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            queryRepoMock.Setup(x => x.GetMaintenancestatusAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>
                {
                    new MaintenanceManagement.Domain.Entities.MiscMaster
                    {
                        Id = 1,
                        MiscTypeId = 1,
                        Code = "TEST",
                        Description = "Test",
                        IsActive = MaintenanceManagement.Domain.Common.BaseEntity.Status.Active,
                        IsDeleted = MaintenanceManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
                    }
                });

            var result = await CreateRepository(ctx, queryRepoMock).UpdateStatusAsync(999999);

            result.Should().BeFalse();
        }

        // --- CommitAsync ---

        [Fact]
        public async Task CommitAsync_Should_Return_Zero_When_No_Pending_Changes()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).CommitAsync(CancellationToken.None);

            result.Should().Be(0);
        }
    }
}
