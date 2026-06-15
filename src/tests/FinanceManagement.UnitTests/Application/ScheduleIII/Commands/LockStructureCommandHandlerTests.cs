using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.LockStructure;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class LockStructureCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private LockStructureCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Locked_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(1)).ReturnsAsync(true);

            var result = await CreateSut().Handle(new LockStructureCommand { StructureId = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Locked_PublishesLockAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(1)).ReturnsAsync(true);

            await CreateSut().Handle(new LockStructureCommand { StructureId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Lock" && e.ActionCode == "S3_STRUCTURE_LOCK"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFoundOrNoStatus_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.LockStructureAsync(99)).ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new LockStructureCommand { StructureId = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
