using Contracts.Commands.Workflow;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class RecurringJournalGenerationServiceTests
    {
        private readonly Mock<IRecurringGenerationRepository> _mockGenRepo = new(MockBehavior.Loose);
        private readonly Mock<IJournalQueryRepository> _mockJournalQuery = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFy = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);

        private RecurringJournalGenerationService CreateSut() =>
            new(_mockGenRepo.Object, _mockJournalQuery.Object, _mockFy.Object, _mockTz.Object,
                _mockOutbox.Object, _mockWorkflow.Object);

        private void SetupCommon(bool periodOpen = true, bool workflowConfigured = true)
        {
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(workflowConfigured);
            _mockJournalQuery.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(periodOpen ? ((int, int)?)(4, 3) : null);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("APPROVED")).ReturnsAsync(102);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockJournalQuery.Setup(r => r.GetSourceIdAsync("RECURRING")).ReturnsAsync(111);
            _mockFy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            _mockGenRepo.Setup(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(50);
        }

        private static RecurringJournalTemplateHeader Template(bool lowRisk) => new()
        {
            Id = 1,
            VoucherTypeId = 1,
            TemplateName = "Monthly Rent",
            AutoPost = true,
            LowRisk = lowRisk,
            Lines = new List<RecurringJournalTemplateDetail>
            {
                new() { LineNo = 1, GlAccountId = 5400101, DrAmount = 150000m, CostCentreId = 1, ProfitCentreId = 1 },
                new() { LineNo = 2, GlAccountId = 2200105, CrAmount = 150000m, ProfitCentreId = 1 }
            }
        };

        [Fact]
        public async Task LowRisk_Template_CreatesApproved_NeverPosted_NoApproval()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(lowRisk: true));
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), CancellationToken.None);

            journalId.Should().Be(50);
            // Low-risk → created APPROVED (status 102), never posted, no approval request raised.
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(
                It.IsAny<JournalHeader>(),
                It.Is<RecurringGenerationLog>(g => g.CompanyId == 1 && g.TemplateId == 1 && g.Period == "4" && !g.AutoPosted),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockOutbox.Verify(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HighRisk_WorkflowConfigured_CreatesDraft_RaisesApproval()
        {
            SetupCommon(workflowConfigured: true);
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(lowRisk: false));
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), CancellationToken.None);

            journalId.Should().Be(50);
            // High-risk + workflow configured → submitted to the JournalVoucher approval workflow.
            _mockOutbox.Verify(o => o.ScheduleAsync(
                It.Is<CreateApprovalRequestCommand>(c => c.ModuleTypeName == MiscEnumEntity.JournalVoucher && c.ModuleTransactionId == 50),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HighRisk_NoWorkflow_StaysDraft_NoApproval()
        {
            SetupCommon(workflowConfigured: false);   // no JournalVoucher workflow → must NOT auto-approve
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(lowRisk: false));
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), CancellationToken.None);

            journalId.Should().Be(50);
            _mockOutbox.Verify(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AlreadyGenerated_Skips()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), CancellationToken.None);

            journalId.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PeriodNotOpen_GeneratesNothing()
        {
            SetupCommon(periodOpen: false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), CancellationToken.None);

            journalId.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
