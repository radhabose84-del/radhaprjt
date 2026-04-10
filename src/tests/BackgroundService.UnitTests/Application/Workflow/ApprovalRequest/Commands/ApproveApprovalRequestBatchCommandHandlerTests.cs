using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Commands
{
    public sealed class ApproveApprovalRequestBatchCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IEventPublisher> _mockEventPublisher = new(MockBehavior.Loose);
        private readonly Mock<IApprovalRequestCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQuery = new(MockBehavior.Loose);
        private readonly Mock<ISalesInvoiceLookup> _mockSalesInvoiceLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ApproveApprovalRequestBatchCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private ApproveApprovalRequestBatchCommandHandler CreateSut() =>
            new(
                _mockMediator.Object,
                _mockEventPublisher.Object,
                _mockCommand.Object,
                _mockMiscMasterQuery.Object,
                _mockSalesInvoiceLookup.Object,
                _mockLogger.Object);

        [Fact]
        public void CanBeConstructed()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NullItems_ReturnsEmptyResult()
        {
            var sut = CreateSut();
            var command = new ApproveApprovalRequestBatchCommand { Items = null! };

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_EmptyItems_ReturnsEmptyResult()
        {
            var sut = CreateSut();
            var command = new ApproveApprovalRequestBatchCommand { Items = new List<ApproveApprovalRequestItemDto>() };

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_InvalidItem_IncrementsSkippedCount()
        {
            var sut = CreateSut();
            var command = new ApproveApprovalRequestBatchCommand
            {
                Items = new List<ApproveApprovalRequestItemDto>
                {
                    new() { ApprovalRequestHeaderId = 0, ModuleTransactionId = 0 }
                }
            };

            var result = await sut.Handle(command, CancellationToken.None);

            result.Total.Should().Be(1);
            result.SkippedCount.Should().Be(1);
        }
    }
}
