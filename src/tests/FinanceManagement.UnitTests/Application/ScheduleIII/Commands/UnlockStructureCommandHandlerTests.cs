using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.UnlockStructure;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class UnlockStructureCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UnlockStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Unlocked_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.UnlockStructureAsync(5)).ReturnsAsync(true);

            var result = await CreateSut().Handle(new UnlockStructureCommand { ScheduleIIIHeaderId = 5 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Unlocked_PublishesUnlockAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.UnlockStructureAsync(5)).ReturnsAsync(true);

            await CreateSut().Handle(new UnlockStructureCommand { ScheduleIIIHeaderId = 5 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Unlock" && e.ActionCode == "S3_STRUCTURE_UNLOCK"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundOrNotLocked_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.UnlockStructureAsync(99)).ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new UnlockStructureCommand { ScheduleIIIHeaderId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
