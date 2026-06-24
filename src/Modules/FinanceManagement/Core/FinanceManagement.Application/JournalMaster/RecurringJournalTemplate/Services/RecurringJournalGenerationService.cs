using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Domain.Entities;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services
{
    // US-GL01-11B — generates journals from due recurring templates for a period.
    // Idempotent per (template, period); auto-posts AutoPost + LowRisk templates; links each
    // generated journal back to its template via RecurringGenerationLog.
    public class RecurringJournalGenerationService : IRecurringJournalGenerationService
    {
        private const int SystemUserId = 0;

        private readonly IRecurringGenerationRepository _generationRepository;
        private readonly IJournalCommandRepository _journalCommandRepository;
        private readonly IJournalQueryRepository _journalQueryRepository;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ITimeZoneService _timeZoneService;

        public RecurringJournalGenerationService(
            IRecurringGenerationRepository generationRepository,
            IJournalCommandRepository journalCommandRepository,
            IJournalQueryRepository journalQueryRepository,
            IFinancialYearLookup financialYearLookup,
            ITimeZoneService timeZoneService)
        {
            _generationRepository = generationRepository;
            _journalCommandRepository = journalCommandRepository;
            _journalQueryRepository = journalQueryRepository;
            _financialYearLookup = financialYearLookup;
            _timeZoneService = timeZoneService;
        }

        public async Task<int> GenerateForPeriodAsync(int companyId, int baseCurrencyId, string period, DateOnly voucherDate, CancellationToken ct)
        {
            // Cannot generate into a non-open period — nothing to do (the job retries on the next run).
            var periodInfo = await _journalQueryRepository.GetOpenPeriodByDateAsync(companyId, voucherDate);
            if (periodInfo == null)
                return 0;

            var (periodId, financialYearId) = periodInfo.Value;
            var draftStatusId = await _journalQueryRepository.GetStatusIdAsync("DRAFT");
            var postedStatusId = await _journalQueryRepository.GetStatusIdAsync("POSTED");
            var sourceId = await _journalQueryRepository.GetSourceIdAsync("RECURRING");
            var fyName = (await _financialYearLookup.GetByIdAsync(financialYearId, ct))?.FinancialYearName;
            var now = _timeZoneService.GetCurrentTime();

            var templates = await _generationRepository.GetDueTemplatesAsync(voucherDate, ct);
            var generated = 0;

            foreach (var template in templates)
            {
                if (await _generationRepository.GenerationExistsAsync(companyId, template.Id, period, ct))
                    continue;   // already generated for this company + period (idempotent)

                var header = BuildJournal(template, companyId, baseCurrencyId, voucherDate, financialYearId, periodId, draftStatusId, sourceId);
                var log = new RecurringGenerationLog
                {
                    CompanyId = companyId,
                    TemplateId = template.Id,
                    Period = period,
                    GeneratedAt = now,
                    AutoPosted = false
                };

                // Journal + log commit atomically — the log is the idempotency guard, so it must never
                // lag the journal it guards. AutoPosted is flipped only after a confirmed post below.
                var journalId = await _generationRepository.CreateJournalWithLogAsync(header, log, ct);

                if (template.AutoPost && template.LowRisk)
                {
                    var result = await _journalCommandRepository.PostAsync(journalId, postedStatusId, fyName, "System", SystemUserId, now, ct);
                    if (result != null)
                        await _generationRepository.MarkLogAutoPostedAsync(log.Id, ct);
                }

                generated++;
            }

            return generated;
        }

        private static JournalHeader BuildJournal(
            RecurringJournalTemplateHeader template, int companyId, int baseCurrencyId, DateOnly voucherDate,
            int financialYearId, int periodId, int draftStatusId, int sourceId)
        {
            var lines = (template.Lines ?? new List<RecurringJournalTemplateDetail>())
                .OrderBy(l => l.LineNo)
                .Select((l, i) =>
                {
                    var dr = l.DrAmount ?? 0m;
                    var cr = l.CrAmount ?? 0m;
                    return new JournalDetail
                    {
                        LineNo = i + 1,
                        GlAccountId = l.GlAccountId,
                        DrAmount = dr,
                        CrAmount = cr,
                        CurrencyId = baseCurrencyId,
                        ExchangeRate = 1m,
                        BaseDrAmount = dr,
                        BaseCrAmount = cr,
                        CostCentreId = l.CostCentreId,
                        ProfitCentreId = l.ProfitCentreId,
                        LineNarration = l.LineNarration
                    };
                })
                .ToList();

            return new JournalHeader
            {
                CompanyId = companyId,
                VoucherTypeId = template.VoucherTypeId,
                VoucherDate = voucherDate,
                FinancialYearId = financialYearId,
                AccountingPeriodId = periodId,
                Narration = template.TemplateName,
                StatusId = draftStatusId,
                SourceId = sourceId,
                IsReversal = false,
                AutoApproved = false,
                TotalDr = lines.Sum(x => x.DrAmount),
                TotalCr = lines.Sum(x => x.CrAmount),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = lines
            };
        }
    }
}
