using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IOutbox;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Commands
{
    public sealed class MoveAccountGroupCommandHandlerTests
    {
        private readonly Mock<IAccountGroupChangeRequestRepository> _mockChangeRequestRepo = new(MockBehavior.Strict);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MoveAccountGroupCommandHandler CreateSut() =>
            new(_mockChangeRequestRepo.Object, _mockOutbox.Object, _mockIp.Object, _mockMediator.Object);

        private static MoveAccountGroupCommand ValidCommand() =>
            new() { Id = 10, NewParentAccountGroupId = 5, Justification = "Restructure for FY2026 reporting", ApproverId = 99 };

        private void SetupHappyPath()
        {
            _mockChangeRequestRepo
                .Setup(r => r.AddWithoutSaveAsync(It.IsAny<FinanceManagement.Domain.Entities.AccountGroupChangeRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockChangeRequestRepo
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSubmittedForApproval()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("submitted");
            result.Data.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_PersistsPendingRequestAndSavesOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockChangeRequestRepo.Verify(
                r => r.AddWithoutSaveAsync(It.IsAny<FinanceManagement.Domain.Entities.AccountGroupChangeRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockChangeRequestRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SchedulesApprovalRequestOnOutbox()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockOutbox.Verify(
                o => o.ScheduleWithoutSaveAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesMoveRequestedAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Move" &&
                        e.ActionCode == "ACCOUNT_GROUP_MOVE_REQUESTED"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
