using Contracts.Commands.Workflow;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.Domain.Common;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate.Commands
{
    public sealed class CreateRecurringJournalTemplateCommandHandlerTests
    {
        private readonly Mock<IRecurringJournalTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRecurringJournalTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private const int PendingId = 20;

        private CreateRecurringJournalTemplateCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockOutbox.Object,
                _mockMediator.Object, _mockMapper.Object);

        private FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader SetupHappyPath(int newId = 1)
        {
            var entity = new FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader();
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>(It.IsAny<CreateRecurringJournalTemplateCommand>()))
                .Returns(entity);
            _mockQueryRepo.Setup(r => r.GetApprovalStatusIdAsync(MiscEnumEntity.Pending)).ReturnsAsync(PendingId);
            _mockIp.Setup(s => s.GetUnitId()).Returns(3);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>()))
                .ReturnsAsync(newId);
            return entity;
        }

        [Theory]
        [InlineData(true, true)]    // AutoPost + LowRisk
        [InlineData(true, false)]   // AutoPost + high-risk
        [InlineData(false, false)]  // manual
        public async Task Handle_AnyFlags_AlwaysPending_RaisesApproval(bool autoPost, bool lowRisk)
        {
            var entity = SetupHappyPath(8);
            CreateApprovalRequestCommand? captured = null;
            _mockOutbox.Setup(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<CreateApprovalRequestCommand, Guid, CancellationToken>((c, _, _) => captured = c)
                .Returns(Task.CompletedTask);

            var cmd = RecurringTemplateBuilders.ValidCreateCommand();
            cmd.AutoPost = autoPost;
            cmd.LowRisk = lowRisk;

            var result = await CreateSut().Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("submitted for approval");
            entity.StatusId.Should().Be(PendingId);   // every template is Pending regardless of flags
            captured.Should().NotBeNull();
            captured!.ModuleTypeName.Should().Be(MiscEnumEntity.RecurringJournalTemplate);
            captured.ModuleTransactionId.Should().Be(8);
        }

        [Fact]
        public async Task Handle_BuildsLines()
        {
            var entity = SetupHappyPath();
            await CreateSut().Handle(RecurringTemplateBuilders.ValidCreateCommand(), CancellationToken.None);

            entity.Lines.Should().HaveCount(2);
            entity.Lines!.First().LineNo.Should().Be(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
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
