using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.MaintenanceRequest;

namespace MaintenanceManagement.IntegrationTests.Repositories.MaintenanceRequest
{
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceRequestQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceRequestQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceRequestQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceRequestQueryRepository(conn, _fixture.IpMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetMaintenancestatusAsync ---

        [Fact]
        public async Task GetMaintenancestatusAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenancestatusAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceOpenstatusAsync ---

        [Fact]
        public async Task GetMaintenanceOpenstatusAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceOpenstatusAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceRequestTypeAsync ---

        [Fact]
        public async Task GetMaintenanceRequestTypeAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceRequestTypeAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetMaintenanceStatusDescAsync ---

        [Fact]
        public async Task GetMaintenanceStatusDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMaintenanceStatusDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetWOclosedAsync ---

        [Fact]
        public async Task GetWOclosedAsync_Should_Return_False_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWOclosedAsync(999999);

            result.Should().BeFalse();
        }

        // --- GetWOclosedOrInProgressAsync ---

        [Fact]
        public async Task GetWOclosedOrInProgressAsync_Should_Return_False_When_No_Data()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWOclosedOrInProgressAsync(999999);

            result.Should().BeFalse();
        }

        // --- GetMachineInfoAsync ---

        [Fact]
        public async Task GetMachineInfoAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetMachineInfoAsync(999999);

            // Dapper ValueTuple default: returns default (null, 0, 0) when nothing — result is (null,0,0)
            // The return type is Nullable<tuple> so null is possible.
            // We accept either null or default-valued tuple.
            (result == null || result.Value.Id == 0).Should().BeTrue();
        }
    }
}
