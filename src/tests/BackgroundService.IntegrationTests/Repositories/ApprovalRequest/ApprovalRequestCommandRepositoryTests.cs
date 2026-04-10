using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests;

namespace BackgroundService.IntegrationTests.Repositories.ApprovalRequest
{
    [Collection("DatabaseCollection")]
    public sealed class ApprovalRequestCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalRequestCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var ctx = _fixture.CreateFreshDbContext();
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            var eventPubMock = new Mock<IEventPublisher>(MockBehavior.Loose);
            var lookupMock = new Mock<ILookupRepository>(MockBehavior.Loose);
            var repo = new ApprovalRequestCommandRepository(ctx, conn, ipMock.Object, eventPubMock.Object, lookupMock.Object);
            repo.Should().NotBeNull();
        }
    }
}
