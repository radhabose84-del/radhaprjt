using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests;

namespace BackgroundService.IntegrationTests.Repositories.ApprovalRequest
{
    [Collection("DatabaseCollection")]
    public sealed class ApprovalRequestQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ApprovalRequestQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            var loggerMock = new Mock<ILogger<ApprovalRequestQueryRepository>>(MockBehavior.Loose);
            var mapperMock = new Mock<IMapper>(MockBehavior.Loose);
            var repo = new ApprovalRequestQueryRepository(conn, ipMock.Object, loggerMock.Object, mapperMock.Object);
            repo.Should().NotBeNull();
        }
    }
}
