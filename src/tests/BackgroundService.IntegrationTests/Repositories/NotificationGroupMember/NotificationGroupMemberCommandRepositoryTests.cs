using BackgroundService.Infrastructure.Repositories.Notification.NotificationGroupMember;

namespace BackgroundService.IntegrationTests.Repositories.NotificationGroupMember
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationGroupMemberCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationGroupMemberCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Repository_Should_Be_Instantiable()
        {
            var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationGroupMemberCommandRepository(ctx);
            repo.Should().NotBeNull();
        }
    }
}
