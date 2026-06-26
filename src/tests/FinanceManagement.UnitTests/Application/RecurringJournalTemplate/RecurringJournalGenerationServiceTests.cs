using Contracts.Commands.Workflow;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Application.RecurringJournalTemplate
{
    public sealed class RecurringJournalGenerationServiceTests
    {
        private readonly Mock<IRecurringGenerationRepository> _mockGenRepo = new(MockBehavior.Loose);
        private readonly Mock<IJournalCommandRepository> _mockJournalCmd = new(MockBehavior.Loose);
        private readonly Mock<IJournalQueryRepository> _mockJournalQuery = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFy = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IWorkflowLookup> _mockWorkflow = new(MockBehavior.Loose);

        private RecurringJournalGenerationService CreateSut() =>
            new(_mockGenRepo.Object, _mockJournalCmd.Object, _mockJournalQuery.Object, _mockFy.Object, _mockTz.Object,
                _mockOutbox.Object, _mockWorkflow.Object);

        private void SetupCommon(bool periodOpen = true, bool workflowConfigured = true)
        {
            _mockWorkflow.Setup(w => w.IsApproveWorkflowConfigureAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(workflowConfigured);
            _mockJournalQuery.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(periodOpen ? ((int, int)?)(4, 3) : null);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("APPROVED")).ReturnsAsync(102);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _mockJournalQuery.Setup(r => r.GetSourceIdAsync("RECURRING")).ReturnsAsync(111);
            _mockFy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            _mockGenRepo.Setup(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(50);
        }

        private static RecurringJournalTemplateHeader Template(bool autoPost, bool lowRisk) => new()
        {
            Id = 1,
            VoucherTypeId = 1,
            TemplateName = "Monthly Rent",
            AutoPost = autoPost,
            LowRisk = lowRisk,
            Lines = new List<RecurringJournalTemplateDetail>
            {
                new() { LineNo = 1, GlAccountId = 5400101, DrAmount = 150000m, CostCentreId = 1, ProfitCentreId = 1 },
                new() { LineNo = 2, GlAccountId = 2200105, CrAmount = 150000m, ProfitCentreId = 1 }
            }
        };

        [Fact]
        public async Task HighRisk_WorkflowConfigured_CreatesDraft_RaisesApproval_NoPost()
        {
            SetupCommon(workflowConfigured: true);
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(autoPost: true, lowRisk: false));   // high-risk → approval, never posts
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // autoPost: true (Hangfire job) — high-risk is still routed to approval, never posted.
            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: true, CancellationToken.None);

            journalId.Should().Be(50);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(
                It.IsAny<JournalHeader>(),
                It.Is<RecurringGenerationLog>(g => g.CompanyId == 1 && g.TemplateId == 1 && g.Period == "4" && !g.AutoPosted),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()), Times.Once);
            // High-risk + workflow configured → submitted to the JournalVoucher approval workflow, not posted.
            _mockOutbox.Verify(o => o.ScheduleAsync(
                It.Is<CreateApprovalRequestCommand>(c => c.ModuleTypeName == MiscEnumEntity.JournalVoucher && c.ModuleTransactionId == 50),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockJournalCmd.Verify(r => r.PostAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>(), It.IsAny<DateOnly?>()), Times.Never);
            _mockGenRepo.Verify(r => r.MarkLogAutoPostedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HighRisk_NoWorkflow_StaysDraft_NoApproval_NoPost()
        {
            SetupCommon(workflowConfigured: false);   // no JournalVoucher workflow → must NOT auto-approve
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(autoPost: true, lowRisk: false));
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: true, CancellationToken.None);

            journalId.Should().Be(50);
            // Stays DRAFT — no approval request raised (so the engine can't auto-approve) and no posting.
            _mockOutbox.Verify(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockJournalCmd.Verify(r => r.PostAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>(), It.IsAny<DateOnly?>()), Times.Never);
        }

        [Fact]
        public async Task LowRisk_NoAutoPost_Template_CreatesApproved_NoPost_NoApproval()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(autoPost: false, lowRisk: true));   // low-risk → APPROVED, manual-post later
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            // autoPost: false (Generate button) — low-risk created APPROVED but NOT posted.
            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: false, CancellationToken.None);

            journalId.Should().Be(50);
            _mockJournalCmd.Verify(r => r.PostAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>(), It.IsAny<DateOnly?>()), Times.Never);
            _mockOutbox.Verify(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AutoPostLowRisk_Template_Posts()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetTemplateByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Template(autoPost: true, lowRisk: true));
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockJournalCmd.Setup(r => r.PostAsync(50, 105, "2026-27", "System", 0, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>(), It.IsAny<DateOnly?>()))
                .ReturnsAsync(new PostJournalResultDto { JournalId = 50, VoucherNo = "JV/2026-27/0001" });

            // autoPost: true (Hangfire job) — low-risk is posted immediately.
            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: true, CancellationToken.None);

            journalId.Should().Be(50);
            _mockJournalCmd.Verify(r => r.PostAsync(50, 105, "2026-27", "System", 0, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>(), It.IsAny<DateOnly?>()), Times.Once);
            _mockGenRepo.Verify(r => r.MarkLogAutoPostedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            // Low-risk → no approval workflow.
            _mockOutbox.Verify(o => o.ScheduleAsync(It.IsAny<CreateApprovalRequestCommand>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AlreadyGenerated_Skips()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "4", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: true, CancellationToken.None);

            journalId.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PeriodNotOpen_GeneratesNothing()
        {
            SetupCommon(periodOpen: false);

            var journalId = await CreateSut().GenerateForTemplateAsync(1, 1, new DateOnly(2026, 6, 1), autoPost: true, CancellationToken.None);

            journalId.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
