using Contracts.Commands.Workflow;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.Domain.Common;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Commands
{
    public sealed class CreateJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFy = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateJournalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockFy.Object, _mockWorkflow.Object,
                _mockOutbox.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int PeriodId, int FinancialYearId)?)(4, 3));
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockQueryRepo.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _mockFy.Setup(f => f.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<CreateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>(), It.IsAny<string?>(), It.IsAny<int>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("draft");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 77);
            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsHeaderTotalsAndContextFromSessionAndPeriod()
        {
            FinanceManagement.Domain.Entities.JournalHeader? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(9);
            _mockIp.Setup(s => s.GetUnitId()).Returns(5);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(9, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockQueryRepo.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _mockFy.Setup(f => f.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<CreateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>(), It.IsAny<string?>(), It.IsAny<int>()))
                .Callback<FinanceManagement.Domain.Entities.JournalHeader, string?, int>((e, _, _) => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(9);
            captured.UnitId.Should().Be(5);
            captured.AccountingPeriodId.Should().Be(4);
            captured.FinancialYearId.Should().Be(3);
            captured.StatusId.Should().Be(101);
            captured.SourceId.Should().Be(111);
            captured.TotalDr.Should().Be(1000m);
            captured.TotalCr.Should().Be(1000m);
            captured.Details.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            var act = async () => await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_NoOpenPeriod_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)null);

            var act = async () => await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "JOURNAL_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WorkflowConfigured_RaisesApprovalRequest()
        {
            SetupHappyPath(newId: 55);
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(55))
                .ReturnsAsync(new JournalHeaderDto { Id = 55, UnitId = 1 });

            CreateApprovalRequestCommand? captured = null;
            _mockOutbox.Setup(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Callback<CreateApprovalRequestCommand, Guid, CancellationToken>((c, _, _) => captured = c)
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Message.Should().Contain("submitted for approval");
            captured.Should().NotBeNull();
            captured!.ModuleTypeName.Should().Be(MiscEnumEntity.JournalVoucher);
            captured.ModuleTransactionId.Should().Be(55);
        }

        [Fact]
        public async Task Handle_NoWorkflowConfigured_DoesNotRaiseApprovalRequest()
        {
            SetupHappyPath();
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Message.Should().Be("Journal voucher saved as draft.");
            _mockOutbox.Verify(
                o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
