using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Commands.ApproveFreightRfq;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Commands
{
    public sealed class ApproveFreightRfqCommandHandlerTests
    {
        private readonly Mock<IFreightRfqCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ApproveFreightRfqCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockCommandRepo.Setup(r => r.ApproveAsync(5, "ok")).ReturnsAsync(5);

            var result = await CreateSut().Handle(
                new ApproveFreightRfqCommand { FreightRfqId = 5, ApprovalRemarks = "ok" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsApproveOnce()
        {
            _mockCommandRepo.Setup(r => r.ApproveAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(5);

            await CreateSut().Handle(new ApproveFreightRfqCommand { FreightRfqId = 5 }, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.ApproveAsync(5, It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.ApproveAsync(It.IsAny<int>(), It.IsAny<string?>())).ReturnsAsync(5);

            await CreateSut().Handle(new ApproveFreightRfqCommand { FreightRfqId = 5 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
