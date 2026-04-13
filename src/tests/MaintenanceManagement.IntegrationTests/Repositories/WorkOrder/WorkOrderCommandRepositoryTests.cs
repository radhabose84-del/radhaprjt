using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Infrastructure.Repositories.WorkOrder;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace MaintenanceManagement.IntegrationTests.Repositories.WorkOrder
{
    [Collection("DatabaseCollection")]
    public sealed class WorkOrderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public WorkOrderCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private WorkOrderCommandRepository CreateRepository(
            MaintenanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var publishEndpointMock = new Mock<IPublishEndpoint>(MockBehavior.Loose);
            var companyLookupMock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            var unitLookupMock = new Mock<IUnitLookup>(MockBehavior.Loose);

            return new WorkOrderCommandRepository(
                ctx,
                _fixture.IpMock.Object,
                conn,
                publishEndpointMock.Object,
                NullLogger<WorkOrderCommandRepository>.Instance,
                companyLookupMock.Object,
                unitLookupMock.Object,
                _fixture.TzMock.Object);
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).GetByIdAsync(999999);

            result.Should().BeNull();
        }

        // --- GetMiscMasterByCodeAsync ---

        [Fact]
        public async Task GetMiscMasterByCodeAsync_Should_Return_Null_When_Code_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).GetMiscMasterByCodeAsync("NONEXISTENT_CODE_XYZ");

            result.Should().BeNull();
        }

        // --- GetBaseDirectoryItemAsync ---

        [Fact]
        public async Task GetBaseDirectoryItemAsync_Should_Return_Null_When_No_Data()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).GetBaseDirectoryItemAsync();

            result.Should().BeNull();
        }

        // --- UpdateWOImageAsync ---

        [Fact]
        public async Task UpdateWOImageAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateWOImageAsync(999999, "image.png");

            result.Should().BeFalse();
        }

        // --- DeleteWOImageAsync ---

        [Fact]
        public async Task DeleteWOImageAsync_Should_Return_False_When_Image_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteWOImageAsync("missing-image.png");

            result.Should().BeFalse();
        }

        // --- DeleteItemImageAsync ---

        [Fact]
        public async Task DeleteItemImageAsync_Should_Return_False_When_Image_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteItemImageAsync("missing-item-image.png");

            result.Should().BeFalse();
        }

        // --- RemoveWOImageReferenceAsync ---

        [Fact]
        public async Task RemoveWOImageReferenceAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).RemoveWOImageReferenceAsync(999999);

            result.Should().BeFalse();
        }

        // --- UpdateWOItemImageAsync ---

        [Fact]
        public async Task UpdateWOItemImageAsync_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateWOItemImageAsync(999999, "image.png");

            result.Should().BeFalse();
        }
    }
}
