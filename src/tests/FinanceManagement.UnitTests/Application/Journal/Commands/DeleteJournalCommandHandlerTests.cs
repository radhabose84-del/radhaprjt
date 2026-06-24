using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal;

namespace FinanceManagement.UnitTests.Application.Journal.Commands
{
    public sealed class DeleteJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteJournalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteJournalCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteJournalCommand(3), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "SoftDelete" && e.ActionCode == "JOURNAL_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
