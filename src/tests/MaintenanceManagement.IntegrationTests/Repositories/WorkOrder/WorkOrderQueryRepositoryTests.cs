using Dapper;
using Microsoft.Data.SqlClient;
using MaintenanceManagement.Infrastructure.Repositories.WorkOrder;

namespace MaintenanceManagement.IntegrationTests.Repositories.WorkOrder
{
    [Collection("DatabaseCollection")]
    public sealed class WorkOrderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkOrderQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkOrderQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new WorkOrderQueryRepository(conn, _fixture.IpMock.Object, _fixture.TzMock.Object);
        }

        private async Task ClearMiscMasterAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetWORootCauseDescAsync ---

        [Fact]
        public async Task GetWORootCauseDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetWORootCauseDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetWOSourceDescAsync ---

        [Fact]
        public async Task GetWOSourceDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetWOSourceDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetWOStatusDescAsync ---

        [Fact]
        public async Task GetWOStatusDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetWOStatusDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetWOStoreTypeDescAsync ---

        [Fact]
        public async Task GetWOStoreTypeDescAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetWOStoreTypeDescAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetRequestTypeAsync ---

        [Fact]
        public async Task GetRequestTypeAsync_Should_Return_Empty_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetRequestTypeAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        // --- GetBaseDirectoryAsync ---

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Return_Null_When_No_Data()
        {
            await ClearMiscMasterAsync();

            var result = await CreateQueryRepo().GetBaseDirectoryAsync();

            result.Should().BeNull();
        }
    }
}
