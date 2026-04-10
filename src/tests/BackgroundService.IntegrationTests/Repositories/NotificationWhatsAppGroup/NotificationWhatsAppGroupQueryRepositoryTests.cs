using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationWhatsAppGroup;

namespace BackgroundService.IntegrationTests.Repositories.NotificationWhatsAppGroup
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationWhatsAppGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationWhatsAppGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            var repo = new NotificationWhatsAppGroupQueryRepository(conn, ipMock.Object);
            repo.Should().NotBeNull();
        }
    }
}
