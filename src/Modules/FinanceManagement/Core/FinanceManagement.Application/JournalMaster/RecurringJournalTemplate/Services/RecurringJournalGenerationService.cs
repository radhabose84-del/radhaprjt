using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services
{
    // US-GL01-11B — generates journals from recurring templates. Idempotent per (template, period).
    // Recurring vouchers are NEVER auto-posted — status follows the template's LowRisk flag:
    //   • LowRisk     → journal created APPROVED (posted manually later via /post or the postable list).
    //   • NOT LowRisk → journal created DRAFT and submitted for approval (JournalVoucher workflow).
    // Each generated journal is linked back to its template via RecurringGenerationLog.
    public class RecurringJournalGenerationService : IRecurringJournalGenerationService
    {
        private readonly IRecurringGenerationRepository _generationRepository;
        private readonly IJournalQueryRepository _journalQueryRepository;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IWorkflowLookup _workflowLookup;

        public RecurringJournalGenerationService(
            IRecurringGenerationRepository generationRepository,
            IJournalQueryRepository journalQueryRepository,
            IFinancialYearLookup financialYearLookup,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher,
            IWorkflowLookup workflowLookup)
        {
            _generationRepository = generationRepository;
            _journalQueryRepository = journalQueryRepository;
            _financialYearLookup = financialYearLookup;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
            _workflowLookup = workflowLookup;
        }

        public async Task<int> GenerateForTemplateAsync(int companyId, int templateId, DateOnly voucherDate, CancellationToken ct)
        {
            var periodInfo = await _journalQueryRepository.GetOpenPeriodByDateAsync(companyId, voucherDate);
            if (periodInfo == null)
                return 0;   // period not open → caller surfaces the message

            var (periodId, financialYearId) = periodInfo.Value;
            // The idempotency key + RecurringGenerationLog.Period store the accounting PERIOD ID (not a yyyy-MM string).
            var period = periodId.ToString();

            // Idempotent per company + template + period (a soft-deleted journal frees the period — see repo).
            if (await _generationRepository.GenerationExistsAsync(companyId, templateId, period, ct))
                return 0;

            var template = await _generationRepository.GetTemplateByIdAsync(templateId, ct);
            if (template == null)
                return 0;

            // Status by LowRisk: low-risk → APPROVED (posted manually later); high-risk → DRAFT + approval.
            // Recurring vouchers are NEVER auto-posted.
            var approvedStatusId = await _journalQueryRepository.GetStatusIdAsync("APPROVED");
            var draftStatusId = await _journalQueryRepository.GetStatusIdAsync("DRAFT");
            var sourceId = await _journalQueryRepository.GetSourceIdAsync("RECURRING");
            var fyName = (await _financialYearLookup.GetByIdAsync(financialYearId, ct))?.FinancialYearName;
            var now = _timeZoneService.GetCurrentTime();

            var createStatusId = template.LowRisk ? approvedStatusId : draftStatusId;

            // baseCurrencyId = 0 → each line's own CurrencyId is used (templates carry currency per detail line).
            return await GenerateOneAsync(template, companyId, 0, period, voucherDate,
                financialYearId, periodId, createStatusId, sourceId, fyName, now, ct);
        }

        // Build + atomically commit one template's journal (+ log). NEVER auto-posts:
        //   • LowRisk     → leave APPROVED (posted manually later).
        //   • NOT LowRisk → DRAFT + raise the JournalVoucher approval request (when a workflow is configured).
        private async Task<int> GenerateOneAsync(
            RecurringJournalTemplateHeader template, int companyId, int baseCurrencyId, string period, DateOnly voucherDate,
            int financialYearId, int periodId, int createStatusId, int sourceId, string? fyName,
            DateTimeOffset now, CancellationToken ct)
        {
            var header = BuildJournal(template, companyId, baseCurrencyId, voucherDate, financialYearId, periodId, createStatusId, sourceId);
            var log = new RecurringGenerationLog
            {
                CompanyId = companyId,
                TemplateId = template.Id,
                Period = period,
                GeneratedAt = now,
                AutoPosted = false
            };

            // Journal + log commit atomically — the log is the idempotency guard, so it must never lag the
            // journal it guards. The voucher number is allocated here at create time (so even non-posted
            // recurring JVs carry a number).
            var journalId = await _generationRepository.CreateJournalWithLogAsync(header, log, fyName, ct);

            if (!template.LowRisk)
            {
                // High-risk → created DRAFT. Route to approval ONLY when a JournalVoucher workflow is configured
                // for the unit (mirrors CreateJournalCommandHandler); otherwise raising the request would be
                // auto-approved by the engine. The status flips to APPROVED only when the voucher is approved via
                // /api/ApprovalRequest/approve → the Finance ApprovedRejectedConsumer.
                var workflowConfigured = await _workflowLookup.IsApproveWorkflowConfigureAsync(
                    MiscEnumEntity.JournalVoucher, template.UnitId, 0);
                if (workflowConfigured)
                    await RaiseApprovalRequestAsync(journalId, template.UnitId, ct);
            }
            // Low-risk → leave APPROVED; never auto-posted.

            return journalId;
        }

        // Submit the generated DRAFT journal to the approval workflow (mirrors CreateJournalCommandHandler):
        // the BackgroundService Workflow module replies via the Finance ApprovedRejectedConsumer
        // (Approved → APPROVED/postable, Rejected → REJECTED).
        private async Task RaiseApprovalRequestAsync(int journalId, int unitId, CancellationToken ct)
        {
            var header = await _journalQueryRepository.GetByIdAsync(journalId);
            if (header != null)
                header.UnitId = unitId;   // sp_EvaluateApproval reads $.Header.UnitId

            var reverseMap = new JournalApprovalReverseDto { Header = header, Lines = null };
            var serializedPayload = JsonSerializer.Serialize(reverseMap);

            var correlationId = Guid.NewGuid();
            await _outboxEventPublisher.ScheduleAsync(new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.JournalVoucher,
                ModuleTransactionId = journalId,
                Payload = serializedPayload
            }, correlationId, ct);
        }

        private static JournalHeader BuildJournal(
            RecurringJournalTemplateHeader template, int companyId, int baseCurrencyId, DateOnly voucherDate,
            int financialYearId, int periodId, int createStatusId, int sourceId)
        {
            var lines = (template.Lines ?? new List<RecurringJournalTemplateDetail>())
                .OrderBy(l => l.LineNo)
                .Select((l, i) =>
                {
                    var dr = l.DrAmount ?? 0m;
                    var cr = l.CrAmount ?? 0m;
                    var currencyId = l.CurrencyId > 0 ? l.CurrencyId : baseCurrencyId;   // line currency, fallback to base
                    var rate = l.ExchangeRate ?? 1m;
                    return new JournalDetail
                    {
                        LineNo = i + 1,
                        GlAccountId = l.GlAccountId,
                        DrAmount = dr,
                        CrAmount = cr,
                        CurrencyId = currencyId,
                        ExchangeRate = rate,
                        BaseDrAmount = dr * rate,
                        BaseCrAmount = cr * rate,
                        CostCentreId = l.CostCentreId,
                        ProfitCentreId = l.ProfitCentreId,
                        LineNarration = l.LineNarration,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };
                })
                .ToList();

            return new JournalHeader
            {
                CompanyId = companyId,
                UnitId = template.UnitId,                    // stamped from the template (set at create from session)
                VoucherTypeId = template.VoucherTypeId,
                VoucherDate = voucherDate,
                FinancialYearId = financialYearId,
                AccountingPeriodId = periodId,
                Narration = template.TemplateName,
                StatusId = createStatusId,                   // APPROVED (low-risk) or DRAFT (high-risk → approval)
                SourceId = sourceId,
                TriggerDocType = "RECURRING",                // originating doc type (US-07)
                TriggerDocRef = template.TemplateName,       // originating doc ref → the template
                IsReversal = false,
                TotalDr = lines.Sum(x => x.DrAmount),
                TotalCr = lines.Sum(x => x.CrAmount),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = lines
            };
        }
    }
}
