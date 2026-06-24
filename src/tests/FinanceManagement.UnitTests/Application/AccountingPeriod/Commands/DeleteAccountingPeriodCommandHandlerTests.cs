using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod;

namespace FinanceManagement.UnitTests.Application.AccountingPeriod.Commands
{
    public sealed class DeleteAccountingPeriodCommandHandlerTests
    {
        private readonly Mock<IAccountingPeriodCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteAccountingPeriodCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteAccountingPeriodCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteAccountingPeriodCommand(5), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteAccountingPeriodCommand(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "ACCOUNTING_PERIOD_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
