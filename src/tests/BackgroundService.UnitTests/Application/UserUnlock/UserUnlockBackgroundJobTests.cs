using System.Reflection;
using BackgroundService.Application.Interfaces;
using BackgroundService.Infrastructure.Jobs;
using Hangfire;

namespace BackgroundService.UnitTests.Application.UserUnlock
{
    public sealed class UserUnlockBackgroundJobTests
    {
        private readonly Mock<IUserUnlockService> _mockUnlockService = new(MockBehavior.Strict);

        private UserUnlockBackgroundJob CreateSut() => new(_mockUnlockService.Object);

        [Fact]
        public async Task Execute_DelegatesToUserUnlockService_WithSameUserName()
        {
            const string userName = "alice";
            _mockUnlockService.Setup(s => s.UnlockUser(userName)).Returns(Task.CompletedTask);

            await CreateSut().Execute(userName);

            _mockUnlockService.Verify(s => s.UnlockUser(userName), Times.Once);
        }

        [Fact]
        public void UserUnlockBackgroundJob_HasQueueAttribute_WithCorrectQueueName()
        {
            var queueAttr = typeof(UserUnlockBackgroundJob)
                .GetCustomAttribute<QueueAttribute>(inherit: false);

            queueAttr.Should().NotBeNull(
                "UserUnlockBackgroundJob is the type Hangfire schedules — [Queue] must be on this class so jobs route to user_unlock_queue, not the default queue");
            queueAttr!.Queue.Should().Be("user_unlock_queue");
        }
    }
}
