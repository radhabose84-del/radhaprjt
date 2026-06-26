using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services;

namespace FinanceManagement.IntegrationTests.Repositories.RecurringJournalTemplate
{
    // US-GL01-11B — end-to-end generation against the real DB: due template -> generated + auto-posted journal,
    // generation logged, idempotent on re-run.
    [Collection("DatabaseCollection")]
    public sealed class RecurringJournalGenerationServiceTests
    {
        private readonly DbFixture _fixture;

        public RecurringJournalGenerationServiceTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private RecurringJournalGenerationService CreateService(FinanceManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Test Company" } });

            var fy = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            fy.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });

            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);

            var journalQuery = new JournalQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object, fy.Object);

            var outbox = new Mock<IOutboxEventPublisher>(MockBehavior.Loose);
            var workflow = new Mock<Contracts.Interfaces.Lookups.Workflow.IWorkflowLookup>(MockBehavior.Loose);

            return new RecurringJournalGenerationService(
                new RecurringGenerationRepository(ctx),
                new JournalCommandRepository(ctx),
                journalQuery,
                fy.Object,
                tz.Object,
                outbox.Object,
                workflow.Object);
        }

        [Fact]
        public async Task GenerateForTemplate_AutoPosts_Template_AndIsIdempotent()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);

            int templateId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                templateId = await new RecurringJournalTemplateCommandRepository(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            var voucherDate = new DateOnly(2026, 6, 15);

            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await CreateService(ctx).GenerateForTemplateAsync(1, templateId, voucherDate, autoPost: true, CancellationToken.None);

            journalId.Should().BeGreaterThan(0);

            await using (var verify = _fixture.CreateFreshDbContext())
            {
                var log = await verify.RecurringGenerationLog.FirstAsync(g => g.TemplateId == templateId);
                log.GeneratedVoucherId.Should().NotBeNull();
                log.AutoPosted.Should().BeTrue();   // template is AutoPost + LowRisk

                var journal = await verify.JournalHeader.FirstAsync(h => h.Id == log.GeneratedVoucherId);
                journal.VoucherNo.Should().NotBeNull();          // auto-posted → number assigned
                journal.TotalDr.Should().Be(journal.TotalCr);
                (await verify.JournalDetail.CountAsync(d => d.JournalHeaderId == journal.Id)).Should().Be(2);
                (await verify.LedgerBalance.CountAsync()).Should().BeGreaterThan(0);   // posting updated balances
            }

            // Re-run for the same period → idempotent (nothing new).
            int second;
            await using (var ctx = _fixture.CreateFreshDbContext())
                second = await CreateService(ctx).GenerateForTemplateAsync(1, templateId, voucherDate, autoPost: true, CancellationToken.None);

            second.Should().Be(0);
            await using (var verify = _fixture.CreateFreshDbContext())
                (await verify.RecurringGenerationLog.CountAsync(g => g.TemplateId == templateId)).Should().Be(1);
        }

        [Fact]
        public async Task GenerationExists_IsScopedPerCompany()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);

            int templateId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                templateId = await new RecurringJournalTemplateCommandRepository(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            // A generation logged for company 1 must not mark company 2 as already-generated.
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                ctx.RecurringGenerationLog.Add(new FinanceManagement.Domain.Entities.RecurringGenerationLog
                {
                    CompanyId = 1,
                    TemplateId = templateId,
                    Period = "2026-06",
                    GeneratedAt = DateTimeOffset.UtcNow,
                    AutoPosted = false
                });
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var repo = new RecurringGenerationRepository(ctx);
                (await repo.GenerationExistsAsync(1, templateId, "2026-06", CancellationToken.None)).Should().BeTrue();
                (await repo.GenerationExistsAsync(2, templateId, "2026-06", CancellationToken.None)).Should().BeFalse();
            }
        }

        [Fact]
        public async Task GenerationExists_False_When_GeneratedJournal_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);

            int templateId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                templateId = await new RecurringJournalTemplateCommandRepository(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            var voucherDate = new DateOnly(2026, 6, 15);

            int journalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                journalId = await CreateService(ctx).GenerateForTemplateAsync(1, templateId, voucherDate, autoPost: true, CancellationToken.None);

            journalId.Should().BeGreaterThan(0);

            // The log stores the accounting period id — read it back to drive the existence checks.
            string periodKey;
            await using (var verify = _fixture.CreateFreshDbContext())
                periodKey = (await verify.RecurringGenerationLog.FirstAsync(g => g.TemplateId == templateId)).Period!;

            // Before deletion → the period counts as generated.
            await using (var ctx = _fixture.CreateFreshDbContext())
                (await new RecurringGenerationRepository(ctx).GenerationExistsAsync(1, templateId, periodKey, CancellationToken.None))
                    .Should().BeTrue();

            // Soft-delete the generated journal → the period is regeneratable again.
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).SoftDeleteAsync(journalId, CancellationToken.None);

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await new RecurringGenerationRepository(ctx).GenerationExistsAsync(1, templateId, periodKey, CancellationToken.None))
                    .Should().BeFalse();
        }
    }
}
