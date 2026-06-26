using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate.Commands
{
    public sealed class UpdateRecurringJournalTemplateCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRecurringTemplateScheduler> _mockScheduler = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRecurringJournalTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockScheduler.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidCommand_Updates_ReSyncsJob_NoApproval()
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(It.IsAny<UpdateRecurringJournalTemplateCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(RecurringTemplateBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            // No approval flow on edit — status preserved; the Hangfire job is re-synced from current status.
            _mockScheduler.Verify(s => s.SyncAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RECURRING_TEMPLATE_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    public sealed class DeleteRecurringJournalTemplateCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteRecurringJournalTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue_AndPublishesAudit()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteRecurringJournalTemplateCommand(3), CancellationToken.None);

            result.Should().BeTrue();
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "RECURRING_TEMPLATE_DELETE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
