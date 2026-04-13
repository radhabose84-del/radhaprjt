using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.Dashboard;

namespace MaintenanceManagement.IntegrationTests.Repositories.Dashboard
{
    /// <summary>
    /// DashboardQueryRepository is entirely backed by the "Dashboard_Maintenance"
    /// stored procedure which does NOT exist in the freshly-created test database.
    /// These tests verify that:
    ///   1. The repository can be constructed with its real dependencies
    ///   2. Its SP-backed methods throw a SqlException (as expected when the SP is missing)
    ///      rather than a NullReferenceException or other unexpected failure.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class DashboardQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DashboardQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DashboardQueryRepository CreateRepo(Mock<IDepartmentLookup>? deptLookup = null)
        {
            deptLookup ??= BuildDefaultDepartmentLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DashboardQueryRepository(conn, deptLookup.Object, _fixture.IpMock.Object);
        }

        private static Mock<IDepartmentLookup> BuildDefaultDepartmentLookup()
        {
            var mock = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new DepartmentLookupDto { DepartmentId = 1, DepartmentName = "Test Dept" }
                });
            return mock;
        }

        [Fact]
        public void Constructor_Should_Not_Throw_With_Valid_Dependencies()
        {
            var act = () => CreateRepo();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task WorkOrderSummaryAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.WorkOrderSummaryAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "", "");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task ItemConsumptionSummaryAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.ItemConsumptionSummaryAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "", "");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task MaintenanceHoursDeptAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.MaintenanceHoursDeptAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "Dept", "");

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task MaintenanceHoursMachineGroupAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.MaintenanceHoursMachineGroupAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "MachineGroup", null);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task MaintenanceHoursMachineAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.MaintenanceHoursMachineAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "Machine", null, null);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task ItemConsumptionDeptSummaryAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.ItemConsumptionDeptSummaryAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "Dept", "", null);

            await act.Should().ThrowAsync<SqlException>();
        }

        [Fact]
        public async Task GetCardDashboardAsync_Should_Throw_SqlException_When_SP_Missing()
        {
            var repo = CreateRepo();

            Func<Task> act = async () =>
                await repo.GetCardDashboardAsync(
                    DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, "Card", "", "");

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
