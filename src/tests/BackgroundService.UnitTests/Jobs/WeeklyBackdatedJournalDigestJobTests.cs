using BackgroundService.Infrastructure.Jobs;
using Contracts.Dtos.Lookups.Users;
using Contracts.Events.Notifications;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Application.JournalMaster.Dto;
using FluentAssertions;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BackgroundService.UnitTests.Jobs
{
    /// <summary>
    /// US-GL03-04 / AC#4 — weekly CFO + FC digest. Verifies skip-safe behaviour when:
    ///   * options are unconfigured (no CFO/FC roles)
    ///   * no recipients resolve from the roles
    ///   * a company has no backdated entries in the window
    /// And verifies an email IS sent per recipient when matches exist.
    /// </summary>
    public sealed class WeeklyBackdatedJournalDigestJobTests
    {
        private readonly Mock<ICompanyLookup>           _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IRoleUserLookup>          _mockRoleLookup    = new(MockBehavior.Loose);
        private readonly Mock<IJournalQueryRepository>  _mockQueryRepo     = new(MockBehavior.Loose);
        private readonly Mock<IMediator>                _mockMediator      = new(MockBehavior.Loose);

        private BackdateDigestOptions _options = new()
        {
            CfoRoleId = 10,
            FcRoleId  = 11,
            WindowDays = 7
        };

        private IServiceScopeFactory BuildScopeFactory()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_mockCompanyLookup.Object);
            services.AddSingleton(_mockRoleLookup.Object);
            services.AddSingleton(_mockQueryRepo.Object);
            services.AddSingleton(_mockMediator.Object);
            services.AddSingleton<IOptions<BackdateDigestOptions>>(Options.Create(_options));
            return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }

        private WeeklyBackdatedJournalDigestJob CreateSut() =>
            new(BuildScopeFactory(), NullLogger<WeeklyBackdatedJournalDigestJob>.Instance);

        private static LatePostingReportDto SampleBackdatedRow(int id = 1) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                CompanyName = "Acme",
                VoucherTypeId = 10,
                VoucherTypeName = "JV",
                VoucherNo = $"JV/2026-27/{id:D4}",
                VoucherDate = new DateOnly(2026, 6, 1),
                AccountingPeriodId = 1,
                AccountingPeriodName = "Jun-2026",
                AccountingPeriodStatusCode = "OPEN",
                StatusId = 200,
                StatusCode = "POSTED",
                StatusName = "Posted",
                PostedAt = new DateTimeOffset(2026, 6, 10, 9, 0, 0, TimeSpan.Zero),
                PostedBy = "fmgr",
                IsBackdated = true,
                DaysBackdated = 9,
                BackdateReason = "Late receipt of bank statement",
                TotalDr = 1000m, TotalCr = 1000m,
                Narration = "Test"
            };

        // ─── Skip-safe branches ──────────────────────────────────────────────

        [Fact]
        public async Task ProcessAsync_NoRolesConfigured_SkipsRun()
        {
            _options = new BackdateDigestOptions { CfoRoleId = 0, FcRoleId = 0 };

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCompanyLookup.Verify(c => c.GetAllCompanyAsync(), Times.Never);
            _mockMediator.Verify(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_NoRecipients_SkipsRun()
        {
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCompanyLookup.Verify(c => c.GetAllCompanyAsync(), Times.Never);
            _mockMediator.Verify(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_NoBackdatedRows_SendsNoEmail()
        {
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "cfo@acme.com" });
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme" } });
            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(
                    It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LatePostingReportDto>());

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockMediator.Verify(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ─── Happy path: send digest to each recipient ───────────────────────

        [Fact]
        public async Task ProcessAsync_BackdatedRowsExist_SendsOneEmailPerRecipient()
        {
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "cfo@acme.com" });
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "fc@acme.com" });
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme" } });
            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(
                    1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LatePostingReportDto> { SampleBackdatedRow() });

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockMediator.Verify(m => m.Send(
                It.Is<SendEmailCommand>(c => c.ToEmail == "cfo@acme.com"
                    && c.Subject!.Contains("Acme")
                    && c.HtmlContent!.Contains("JV/2026-27/0001")),
                It.IsAny<CancellationToken>()), Times.Once);

            _mockMediator.Verify(m => m.Send(
                It.Is<SendEmailCommand>(c => c.ToEmail == "fc@acme.com"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_DeduplicatesRecipients_AcrossRoles()
        {
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "boss@acme.com" });
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "BOSS@acme.com" }); // case-difference
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme" } });
            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(1,
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LatePostingReportDto> { SampleBackdatedRow() });

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockMediator.Verify(m => m.Send(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_IncludesCcEmails_AsRecipients()
        {
            _options = new BackdateDigestOptions
            {
                CfoRoleId = 10,
                FcRoleId  = 11,
                WindowDays = 7,
                CcEmails  = new List<string> { "compliance@acme.com" }
            };

            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>());
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Acme" } });
            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(1,
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LatePostingReportDto> { SampleBackdatedRow() });

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockMediator.Verify(m => m.Send(
                It.Is<SendEmailCommand>(c => c.ToEmail == "compliance@acme.com"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─── Per-company isolation ───────────────────────────────────────────

        [Fact]
        public async Task ProcessAsync_OneCompanyFails_OthersStillProcessed()
        {
            _mockRoleLookup.Setup(r => r.GetEmailsByRoleAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { "cfo@acme.com" });
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new() { CompanyId = 1, CompanyName = "Acme" },
                    new() { CompanyId = 2, CompanyName = "Beta" }
                });

            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(1, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));
            _mockQueryRepo
                .Setup(r => r.GetBackdatedJournalsForDigestAsync(2, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LatePostingReportDto> { SampleBackdatedRow() });

            // Should NOT throw — the per-company catch swallows the failure and logs it.
            var act = async () => await CreateSut().ProcessAsync(CancellationToken.None);
            await act.Should().NotThrowAsync();

            // Beta's email still went out.
            _mockMediator.Verify(m => m.Send(
                It.Is<SendEmailCommand>(c => c.Subject!.Contains("Beta")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─── Hangfire metadata ───────────────────────────────────────────────

        [Fact]
        public void ProcessAsync_IsDecoratedWith_QueueAndAutomaticRetry()
        {
            var method = typeof(WeeklyBackdatedJournalDigestJob).GetMethod(nameof(WeeklyBackdatedJournalDigestJob.ProcessAsync))!;

            var queueAttr = method.GetCustomAttributes(typeof(QueueAttribute), inherit: false)
                .OfType<QueueAttribute>().SingleOrDefault();
            queueAttr.Should().NotBeNull();
            queueAttr!.Queue.Should().Be("finance-jobs");

            var retryAttr = method.GetCustomAttributes(typeof(AutomaticRetryAttribute), inherit: false)
                .OfType<AutomaticRetryAttribute>().SingleOrDefault();
            retryAttr.Should().NotBeNull();
            retryAttr!.Attempts.Should().Be(3);
        }
    }
}
