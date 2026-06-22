using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services;
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

        private RecurringJournalGenerationService CreateSut() =>
            new(_mockGenRepo.Object, _mockJournalCmd.Object, _mockJournalQuery.Object, _mockFy.Object, _mockTz.Object);

        private void SetupCommon(bool periodOpen = true)
        {
            _mockJournalQuery.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(periodOpen ? ((int, int)?)(4, 3) : null);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockJournalQuery.Setup(r => r.GetStatusIdAsync("POSTED")).ReturnsAsync(105);
            _mockJournalQuery.Setup(r => r.GetSourceIdAsync("RECURRING")).ReturnsAsync(111);
            _mockFy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            _mockTz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);
            _mockGenRepo.Setup(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<CancellationToken>()))
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
        public async Task DraftOnly_Template_GeneratesDraft_NoPost()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetDueTemplatesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RecurringJournalTemplateHeader> { Template(autoPost: false, lowRisk: false) });
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "2026-06", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var count = await CreateSut().GenerateForPeriodAsync(1, 1, "2026-06", new DateOnly(2026, 6, 1), CancellationToken.None);

            count.Should().Be(1);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(
                It.IsAny<JournalHeader>(),
                It.Is<RecurringGenerationLog>(g => g.CompanyId == 1 && g.TemplateId == 1 && g.Period == "2026-06" && !g.AutoPosted),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockJournalCmd.Verify(r => r.PostAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockGenRepo.Verify(r => r.MarkLogAutoPostedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AutoPostLowRisk_Template_Posts()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetDueTemplatesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RecurringJournalTemplateHeader> { Template(autoPost: true, lowRisk: true) });
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "2026-06", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockJournalCmd.Setup(r => r.PostAsync(50, 105, "2026-27", 0, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PostJournalResultDto { JournalId = 50, VoucherNo = "JV/2026-27/0001" });

            var count = await CreateSut().GenerateForPeriodAsync(1, 1, "2026-06", new DateOnly(2026, 6, 1), CancellationToken.None);

            count.Should().Be(1);
            _mockJournalCmd.Verify(r => r.PostAsync(50, 105, "2026-27", 0, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockGenRepo.Verify(r => r.MarkLogAutoPostedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AlreadyGenerated_Skips()
        {
            SetupCommon();
            _mockGenRepo.Setup(r => r.GetDueTemplatesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RecurringJournalTemplateHeader> { Template(false, false) });
            _mockGenRepo.Setup(r => r.GenerationExistsAsync(1, 1, "2026-06", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var count = await CreateSut().GenerateForPeriodAsync(1, 1, "2026-06", new DateOnly(2026, 6, 1), CancellationToken.None);

            count.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PeriodNotOpen_GeneratesNothing()
        {
            SetupCommon(periodOpen: false);

            var count = await CreateSut().GenerateForPeriodAsync(1, 1, "2026-06", new DateOnly(2026, 6, 1), CancellationToken.None);

            count.Should().Be(0);
            _mockGenRepo.Verify(r => r.CreateJournalWithLogAsync(It.IsAny<JournalHeader>(), It.IsAny<RecurringGenerationLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
