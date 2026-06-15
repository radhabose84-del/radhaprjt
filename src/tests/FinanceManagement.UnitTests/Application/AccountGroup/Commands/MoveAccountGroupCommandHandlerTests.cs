using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Commands
{
    public sealed class MoveAccountGroupCommandHandlerTests
    {
        private readonly Mock<IAccountGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MoveAccountGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private static MoveAccountGroupCommand ValidCommand() =>
            new() { Id = 10, NewParentAccountGroupId = 5, Justification = "Restructure for FY2026 reporting", ApproverId = 99 };

        private void SetupHappyPath(int movedId = 10)
        {
            _mockCommandRepo
                .Setup(r => r.MoveAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(movedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("move processed");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsMoveWithCorrectArguments()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.MoveAsync(10, 5), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesMoveAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Move" &&
                        e.ActionCode == "ACCOUNT_GROUP_MOVE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
