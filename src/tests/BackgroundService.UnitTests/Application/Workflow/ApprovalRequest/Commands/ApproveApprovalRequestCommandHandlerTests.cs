using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackgroundService.UnitTests.Application.Workflow.ApprovalRequest.Commands
{
    public sealed class ApproveApprovalRequestCommandHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterQuery = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddress = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTimeZone = new(MockBehavior.Loose);
        private readonly Mock<IApprovalRequestCommand> _mockCommand = new(MockBehavior.Loose);
        private readonly Mock<IApprovalRequestQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IEventPublisher> _mockEventPublisher = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ApproveApprovalRequestCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private ApproveApprovalRequestCommandHandler CreateSut() =>
            new(
                _mockMiscMasterQuery.Object,
                _mockIpAddress.Object,
                _mockTimeZone.Object,
                _mockCommand.Object,
                _mockQuery.Object,
                _mockEventPublisher.Object,
                _mockMapper.Object,
                _mockLogger.Object);

        [Fact]
        public void CanBeConstructed()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ZeroModuleTransactionId_ThrowsInvalidOperation()
        {
            var sut = CreateSut();
            var command = new ApproveApprovalRequestCommand
            {
                ApprovalRequestHeaderId = 1,
                ModuleTransactionId = 0,
                Remark = "test"
            };

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*ModuleTransactionId*");
        }

        [Fact]
        public async Task Handle_AlreadyApproved_ThrowsInvalidOperation()
        {
            var command = new ApproveApprovalRequestCommand
            {
                ApprovalRequestHeaderId = 1,
                ModuleTransactionId = 10,
                Remark = "test",
                IsApproved = 1
            };

            _mockQuery
                .Setup(q => q.HeaderLevelApprovalStatus(1, 10))
                .Returns(Task.FromResult<dynamic>(null!));

            _mockMapper
                .Setup(m => m.Map<HeaderStatusDto>(It.IsAny<object>()))
                .Returns(new HeaderStatusDto { StatusCode = "Approved" });

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already approved*");
        }
    }
}
