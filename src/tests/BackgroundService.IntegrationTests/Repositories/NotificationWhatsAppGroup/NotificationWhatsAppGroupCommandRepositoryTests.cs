using Contracts.Interfaces;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationWhatsAppGroup;

namespace BackgroundService.IntegrationTests.Repositories.NotificationWhatsAppGroup
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationWhatsAppGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationWhatsAppGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var ctx = _fixture.CreateFreshDbContext();
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            var repo = new NotificationWhatsAppGroupCommandRepository(ctx, ipMock.Object);
            repo.Should().NotBeNull();
        }
    }
}
