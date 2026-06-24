using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Application.Common.Interfaces;
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

            return new RecurringJournalGenerationService(
                new RecurringGenerationRepository(ctx),
                new JournalCommandRepository(ctx),
                journalQuery,
                fy.Object,
                tz.Object);
        }

        [Fact]
        public async Task GenerateForPeriod_AutoPosts_DueTemplate_AndIsIdempotent()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await RecurringTemplateSeed.SeedAsync(_fixture);

            int templateId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                templateId = await new RecurringJournalTemplateCommandRepository(ctx).CreateAsync(RecurringTemplateSeed.BuildTemplate(ids));

            var voucherDate = new DateOnly(2026, 6, 15);

            int generated;
            await using (var ctx = _fixture.CreateFreshDbContext())
                generated = await CreateService(ctx).GenerateForPeriodAsync(1, ids.CurrencyId, "2026-06", voucherDate, CancellationToken.None);

            generated.Should().Be(1);

            await using (var verify = _fixture.CreateFreshDbContext())
            {
                var log = await verify.RecurringGenerationLog.FirstAsync(g => g.TemplateId == templateId && g.Period == "2026-06");
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
                second = await CreateService(ctx).GenerateForPeriodAsync(1, ids.CurrencyId, "2026-06", voucherDate, CancellationToken.None);

            second.Should().Be(0);
            await using (var verify = _fixture.CreateFreshDbContext())
                (await verify.RecurringGenerationLog.CountAsync(g => g.TemplateId == templateId && g.Period == "2026-06")).Should().Be(1);
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
    }
}
