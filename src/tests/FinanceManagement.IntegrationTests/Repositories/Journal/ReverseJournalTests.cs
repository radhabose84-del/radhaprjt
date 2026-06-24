using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using MediatR;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    // US-GL01 reversal — posts a mirror voucher, links it, and marks the original REVERSED.
    [Collection("DatabaseCollection")]
    public sealed class ReverseJournalTests
    {
        private readonly DbFixture _fixture;

        public ReverseJournalTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ReverseJournalCommandHandler CreateHandler(ApplicationDbContext ctx)
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Test Company" } });

            var fy = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            fy.Setup(f => f.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearLookupDto { FinancialYearId = 3, FinancialYearName = "2026-27" });
            fy.Setup(f => f.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>
                {
                    new() { FinancialYearId = 3, FinancialYearName = "2026-27", IsActive = true,
                            StartDate = new DateTime(2000, 1, 1), EndDate = new DateTime(2100, 1, 1) }
                });

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(s => s.GetUserId()).Returns(1);
            ip.Setup(s => s.GetCompanyId()).Returns(1);
            ip.Setup(s => s.GetUnitId()).Returns(1);

            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(t => t.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);

            var mediator = new Mock<IMediator>(MockBehavior.Loose);

            var query = new JournalQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object, fy.Object);
            return new ReverseJournalCommandHandler(
                new JournalCommandRepository(ctx), query, fy.Object, ip.Object, tz.Object, mediator.Object);
        }

        [Fact]
        public async Task Reverse_PostsMirror_LinksIt_AndMarksOriginalReversed()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await JournalTestSeed.SeedGraphAsync(_fixture);

            int originalId;
            await using (var ctx = _fixture.CreateFreshDbContext())
                originalId = await new JournalCommandRepository(ctx).CreateAsync(JournalTestSeed.BuildDraftJournal(ids, amount: 1000m));
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new JournalCommandRepository(ctx).PostAsync(originalId, ids.StatusPostedId, "2026-27", "Tester", 1, DateTimeOffset.UtcNow, CancellationToken.None);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var res = await CreateHandler(ctx).Handle(
                    new ReverseJournalCommand { Id = originalId, ReversalDate = new DateOnly(2026, 6, 15) },
                    CancellationToken.None);

                res.IsSuccess.Should().BeTrue();
                res.Data!.VoucherNo.Should().NotBeNull();
            }

            await using var verify = _fixture.CreateFreshDbContext();

            // Original flipped to REVERSED.
            var original = await verify.JournalHeader.FirstAsync(h => h.Id == originalId);
            original.StatusId.Should().Be(ids.StatusReversedId);

            // Reversal entry: posted, linked, mirrored.
            var reversal = await verify.JournalHeader.Include(h => h.Details).FirstAsync(h => h.ReversalOfId == originalId);
            reversal.IsReversal.Should().BeTrue();
            reversal.VoucherNo.Should().NotBeNull();
            reversal.TotalDr.Should().Be(1000m);
            reversal.TotalCr.Should().Be(1000m);
            reversal.Details.Should().Contain(d => d.GlAccountId == ids.GlAccountDrId && d.CrAmount == 1000m && d.DrAmount == 0m);
            reversal.Details.Should().Contain(d => d.GlAccountId == ids.GlAccountCrId && d.DrAmount == 1000m && d.CrAmount == 0m);

            // Original's line items are kept INTACT for the audit trail (not soft-deleted).
            var originalLines = await verify.JournalDetail.IgnoreQueryFilters()
                .Where(d => d.JournalHeaderId == originalId).ToListAsync();
            originalLines.Should().NotBeEmpty();
            originalLines.Should().OnlyContain(d =>
                d.IsDeleted == FinanceManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);

            // Ledger still nets to zero (the mirror contra-posted; the original's lines are untouched).
            var balances = await verify.LedgerBalance.ToListAsync();
            balances.Sum(b => b.Balance).Should().Be(0m);
        }
    }
}
