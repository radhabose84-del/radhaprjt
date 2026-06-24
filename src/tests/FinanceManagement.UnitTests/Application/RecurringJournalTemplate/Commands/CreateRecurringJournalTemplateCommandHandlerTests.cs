using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate.Commands
{
    public sealed class CreateRecurringJournalTemplateCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateRecurringJournalTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(It.IsAny<CreateRecurringJournalTemplateCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(RecurringTemplateBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_BuildsLines()
        {
            FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader? captured = null;
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(It.IsAny<CreateRecurringJournalTemplateCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()))
                .Callback<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(RecurringTemplateBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.Lines.Should().HaveCount(2);
            captured.Lines!.First().LineNo.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(RecurringTemplateBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "RECURRING_TEMPLATE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
