using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal
{
    public class ReverseJournalCommandHandler : IRequestHandler<ReverseJournalCommand, ApiResponseDTO<PostJournalResultDto>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public ReverseJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IFinancialYearLookup financialYearLookup,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _financialYearLookup = financialYearLookup;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<PostJournalResultDto>> Handle(ReverseJournalCommand request, CancellationToken cancellationToken)
        {
            var original = await _queryRepository.GetByIdAsync(request.Id)
                ?? throw new ExceptionRules("Journal voucher not found.");

            // AC-3 — default the reversal date to the first day of the next open period when not supplied.
            var reversalDate = request.ReversalDate
                ?? await _queryRepository.GetNextOpenPeriodStartAsync(original.CompanyId, original.PostingDate ?? original.VoucherDate)
                ?? throw new ExceptionRules("No open accounting period is available for the reversal.");

            // Validation (posted, not a reversal, not already reversed, date >= posting) runs in the validator.
            var period = await _queryRepository.GetOpenPeriodByDateAsync(original.CompanyId, reversalDate)
                ?? throw new ExceptionRules("Reversal date is not within an open accounting period.");

            var draftStatusId = await _queryRepository.GetStatusIdAsync("DRAFT");
            var postedStatusId = await _queryRepository.GetStatusIdAsync("POSTED");
            var reversedStatusId = await _queryRepository.GetStatusIdAsync("REVERSED");
            var sourceId = await _queryRepository.GetSourceIdAsync("MANUAL");
            var fyName = (await _financialYearLookup.GetByIdAsync(period.FinancialYearId, cancellationToken))?.FinancialYearName;
            var postedById = _ipAddressService.GetUserId();
            var postedByName = _ipAddressService.GetUserName();
            var now = _timeZoneService.GetCurrentTime();

            // Build the mirror voucher (Dr/Cr and base amounts swapped) linked to the original.
            var lineNo = 0;
            var reversal = new JournalHeader
            {
                CompanyId = original.CompanyId,
                UnitId = original.UnitId,
                VoucherTypeId = original.VoucherTypeId,
                VoucherDate = reversalDate,
                FinancialYearId = period.FinancialYearId,
                AccountingPeriodId = period.PeriodId,
                // AC-2 — narration is always prefixed "Reversal of {original voucher no}".
                Narration = string.IsNullOrWhiteSpace(request.Narration)
                    ? $"Reversal of {original.VoucherNo}"
                    : $"Reversal of {original.VoucherNo} — {request.Narration}",
                StatusId = draftStatusId,
                SourceId = sourceId,
                IsReversal = true,
                ReversalOfId = original.Id,
                TotalDr = original.TotalCr,
                TotalCr = original.TotalDr,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = original.Lines.Select(l => new JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = l.GlAccountId,
                    DrAmount = l.CrAmount,
                    CrAmount = l.DrAmount,
                    CurrencyId = l.CurrencyId,
                    ExchangeRate = l.ExchangeRate,
                    BaseDrAmount = l.BaseCrAmount,
                    BaseCrAmount = l.BaseDrAmount,
                    CostCentreId = l.CostCentreId,
                    ProfitCentreId = l.ProfitCentreId,
                    LineNarration = l.LineNarration,
                    ReferenceDocNo = l.ReferenceDocNo,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };

            // Atomic: create + post the mirror and flip the original to REVERSED in one transaction.
            var result = await _commandRepository.ReverseAsync(
                reversal, original.Id, postedStatusId, reversedStatusId, fyName, postedByName, postedById, now, cancellationToken)
                ?? throw new ExceptionRules("Reversal voucher could not be posted.");

            var reversalId = result.JournalId;

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Reverse",
                actionCode: "JOURNAL_REVERSE",
                actionName: result.VoucherNo ?? reversalId.ToString(),
                details: $"Journal voucher {original.Id} ({original.VoucherNo}) reversed by {result.VoucherNo}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // Flagging engine on the posted reversal (e.g. SAME_DAY_REV).
            await _mediator.Publish(
                new JournalPostedDomainEvent(reversalId, reversal.CompanyId, reversal.TotalDr,
                    DateOnly.FromDateTime(now.DateTime), isReversal: true),
                cancellationToken);

            return new ApiResponseDTO<PostJournalResultDto>
            {
                IsSuccess = true,
                Message = $"Journal voucher reversed successfully as {result.VoucherNo}.",
                Data = result
            };
        }
    }
}
