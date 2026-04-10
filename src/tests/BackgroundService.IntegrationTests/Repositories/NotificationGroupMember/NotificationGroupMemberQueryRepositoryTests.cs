using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationGroupMember;

namespace BackgroundService.IntegrationTests.Repositories.NotificationGroupMember
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationGroupMemberQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationGroupMemberQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            var repo = new NotificationGroupMemberQueryRepository(conn, ipMock.Object);
            repo.Should().NotBeNull();
        }
    }
}
