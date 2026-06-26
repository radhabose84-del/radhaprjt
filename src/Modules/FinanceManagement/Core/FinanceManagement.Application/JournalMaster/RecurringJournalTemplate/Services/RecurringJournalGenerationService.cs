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
    // US-GL01-11B — generates journals from due recurring templates for a period.
    // Idempotent per (template, period). Approval routing follows the template's LowRisk flag:
    //   • LowRisk        → journal created APPROVED (postable); AutoPost templates are posted immediately.
    //   • NOT LowRisk    → journal created DRAFT and submitted for approval (JournalVoucher workflow).
    // Each generated journal is linked back to its template via RecurringGenerationLog.
    public class RecurringJournalGenerationService : IRecurringJournalGenerationService
    {
        private const int SystemUserId = 0;

        private readonly IRecurringGenerationRepository _generationRepository;
        private readonly IJournalCommandRepository _journalCommandRepository;
        private readonly IJournalQueryRepository _journalQueryRepository;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IWorkflowLookup _workflowLookup;

        public RecurringJournalGenerationService(
            IRecurringGenerationRepository generationRepository,
            IJournalCommandRepository journalCommandRepository,
            IJournalQueryRepository journalQueryRepository,
            IFinancialYearLookup financialYearLookup,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher,
            IWorkflowLookup workflowLookup)
        {
            _generationRepository = generationRepository;
            _journalCommandRepository = journalCommandRepository;
            _journalQueryRepository = journalQueryRepository;
            _financialYearLookup = financialYearLookup;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
            _workflowLookup = workflowLookup;
        }

        public async Task<int> GenerateForTemplateAsync(int companyId, int templateId, DateOnly voucherDate, bool autoPost, CancellationToken ct)
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

            // Approval routing by LowRisk: low-risk → create APPROVED (postable); high-risk → create DRAFT and
            // submit for approval. (sp_EvaluateApproval reads $.Header.UnitId — supplied from the template.)
            var approvedStatusId = await _journalQueryRepository.GetStatusIdAsync("APPROVED");
            var draftStatusId = await _journalQueryRepository.GetStatusIdAsync("DRAFT");
            var postedStatusId = await _journalQueryRepository.GetStatusIdAsync("POSTED");
            var sourceId = await _journalQueryRepository.GetSourceIdAsync("RECURRING");
            var fyName = (await _financialYearLookup.GetByIdAsync(financialYearId, ct))?.FinancialYearName;
            var now = _timeZoneService.GetCurrentTime();

            var createStatusId = template.LowRisk ? approvedStatusId : draftStatusId;

            // baseCurrencyId = 0 → each line's own CurrencyId is used (templates carry currency per detail line).
            return await GenerateOneAsync(template, companyId, 0, period, voucherDate,
                financialYearId, periodId, createStatusId, postedStatusId, sourceId, fyName, autoPost, now, ct);
        }

        // Build + atomically commit one template's journal (+ log).
        //   • LowRisk + autoPost (Hangfire job)  → post immediately (POSTED).
        //   • LowRisk + !autoPost (Generate btn) → leave APPROVED (posted manually later).
        //   • NOT LowRisk                        → DRAFT + raise the JournalVoucher approval request.
        private async Task<int> GenerateOneAsync(
            RecurringJournalTemplateHeader template, int companyId, int baseCurrencyId, string period, DateOnly voucherDate,
            int financialYearId, int periodId, int createStatusId, int postedStatusId, int sourceId, string? fyName,
            bool autoPost, DateTimeOffset now, CancellationToken ct)
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
            // recurring JVs carry a number). AutoPosted is flipped only after a confirmed post below.
            var journalId = await _generationRepository.CreateJournalWithLogAsync(header, log, fyName, ct);

            if (template.LowRisk)
            {
                // Low-risk → already APPROVED; only the unattended Hangfire auto-post job posts it (autoPost).
                if (autoPost)
                {
                    var result = await _journalCommandRepository.PostAsync(journalId, postedStatusId, fyName, "System", SystemUserId, now, ct);
                    if (result != null)
                        await _generationRepository.MarkLogAutoPostedAsync(log.Id, ct);
                }
            }
            else
            {
                // High-risk → created DRAFT and left DRAFT. Route to approval ONLY when a JournalVoucher workflow is
                // configured for the unit (mirrors CreateJournalCommandHandler); otherwise raising the request would
                // be auto-approved by the engine. The status flips to APPROVED only when the voucher is approved via
                // /api/ApprovalRequest/approve → the Finance ApprovedRejectedConsumer.
                var workflowConfigured = await _workflowLookup.IsApproveWorkflowConfigureAsync(
                    MiscEnumEntity.JournalVoucher, template.UnitId, 0);
                if (workflowConfigured)
                    await RaiseApprovalRequestAsync(journalId, template.UnitId, ct);
            }

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
