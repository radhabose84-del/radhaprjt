using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Commands
{
    public sealed class SubmitFreightRfqForApprovalCommandHandlerTests
    {
        private readonly Mock<IFreightRfqCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

        private SubmitFreightRfqForApprovalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockIp.Object, _mockOutbox.Object);

        private void SetupHappyPath()
        {
            _mockCommandRepo.Setup(r => r.SubmitForApprovalAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string?>()))
                .ReturnsAsync(1);
            _mockCommandRepo.Setup(r => r.GetWorkflowPayloadAsync(It.IsAny<int>()))
                .ReturnsAsync(new FreightRfqWorkFlowDto { Id = 1, FreightRfqNumber = "FRFQ-2026-0003", StatusId = 1225 });
            _mockIp.Setup(s => s.GetUnitId()).Returns(37);
        }

        private static SubmitFreightRfqForApprovalCommand Command() =>
            new() { FreightRfqId = 3, SelectedQuotationId = 5, IsOverride = false, ComparisonRemarks = null };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(Command(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_RaisesWorkflowRequest_RoutedByMenuName()
        {
            SetupHappyPath();

            await CreateSut().Handle(Command(), CancellationToken.None);

            _mockOutbox.Verify(o => o.ScheduleAsync(
                It.Is<CreateApprovalRequestCommand>(c =>
                    c.ModuleTypeName == "Freight RFQ" &&
                    c.ModuleTransactionId == 3 &&
                    c.TransactionTypeId == null),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MarksSelectedAndPending()
        {
            SetupHappyPath();

            await CreateSut().Handle(Command(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SubmitForApprovalAsync(3, 5, false, null), Times.Once);
        }
    }
}
