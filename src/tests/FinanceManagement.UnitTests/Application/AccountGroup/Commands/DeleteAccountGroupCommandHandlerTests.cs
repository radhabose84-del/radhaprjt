using FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Commands
{
    public sealed class DeleteAccountGroupCommandHandlerTests
    {
        private readonly Mock<IAccountGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAccountGroupCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new DeleteAccountGroupCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteAccountGroupCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesSoftDeleteAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new DeleteAccountGroupCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "ACCOUNT_GROUP_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
