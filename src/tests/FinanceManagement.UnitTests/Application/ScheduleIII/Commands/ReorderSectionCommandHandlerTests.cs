using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderSection;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class ReorderSectionCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ReorderSectionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Reordered_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.ReorderSectionAsync(3, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new ReorderSectionCommand { Id = 3, Direction = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Reordered_PublishesReorderAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.ReorderSectionAsync(3, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new ReorderSectionCommand { Id = 3, Direction = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Reorder" && e.ActionCode == "S3_SECTION_REORDER"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NoNeighbour_ThrowsExceptionRules()
        {
            _mockCommandRepo.Setup(r => r.ReorderSectionAsync(99, -1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new ReorderSectionCommand { Id = 99, Direction = -1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
