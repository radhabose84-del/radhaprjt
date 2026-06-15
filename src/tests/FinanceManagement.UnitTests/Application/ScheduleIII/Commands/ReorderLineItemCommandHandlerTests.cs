using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.ReorderLineItem;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Commands
{
    public sealed class ReorderLineItemCommandHandlerTests
    {
        private readonly Mock<IScheduleIIICommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ReorderLineItemCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_SuccessfulReorder_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.ReorderLineItemAsync(14, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(
                new ReorderLineItemCommand { LineItemId = 14, Direction = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NoNeighbour_ReturnsFalse()
        {
            _mockCommandRepo.Setup(r => r.ReorderLineItemAsync(14, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateSut().Handle(
                new ReorderLineItemCommand { LineItemId = 14, Direction = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_PublishesReorderAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.ReorderLineItemAsync(14, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new ReorderLineItemCommand { LineItemId = 14, Direction = 2 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "S3_LINEITEM_REORDER"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
