using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal
{
    public class CopyJournalCommandHandler : IRequestHandler<CopyJournalCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public CopyJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CopyJournalCommand request, CancellationToken cancellationToken)
        {
            var source = await _queryRepository.GetByIdAsync(request.Id)
                ?? throw new ExceptionRules("Journal voucher not found.");

            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // New draft dated today; resolve the open period for that date (same rule as a manual draft).
            var today = DateOnly.FromDateTime(_timeZoneService.GetCurrentTime().DateTime);
            var period = await _queryRepository.GetOpenPeriodByDateAsync(companyId, today)
                ?? throw new ExceptionRules("Today is not within an open accounting period; cannot create the copy.");

            var draftStatusId = await _queryRepository.GetStatusIdAsync("DRAFT");
            var sourceId = await _queryRepository.GetSourceIdAsync("MANUAL");

            var lineNo = 0;
            var copy = new JournalHeader
            {
                CompanyId = companyId,
                UnitId = _ipAddressService.GetUnitId(),
                VoucherTypeId = source.VoucherTypeId,
                VoucherDate = today,
                FinancialYearId = period.FinancialYearId,
                AccountingPeriodId = period.PeriodId,
                Narration = source.Narration,
                StatusId = draftStatusId,
                SourceId = sourceId,
                IsReversal = false,
                AutoApproved = false,
                CopiedFromRef = source.VoucherNo,   // informational only — no posting/FK link
                TotalDr = source.TotalDr,
                TotalCr = source.TotalCr,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = source.Lines.Select(l => new JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = l.GlAccountId,
                    DrAmount = l.DrAmount,
                    CrAmount = l.CrAmount,
                    CurrencyId = l.CurrencyId,
                    ExchangeRate = l.ExchangeRate,
                    BaseDrAmount = l.BaseDrAmount,
                    BaseCrAmount = l.BaseCrAmount,
                    CostCentreId = l.CostCentreId,
                    ProfitCentreId = l.ProfitCentreId,
                    LineNarration = l.LineNarration,
                    ReferenceDocNo = l.ReferenceDocNo,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList()
            };

            var newId = await _commandRepository.CreateAsync(copy);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Copy",
                actionCode: "JOURNAL_COPY",
                actionName: newId.ToString(),
                details: $"Journal voucher {request.Id} ({source.VoucherNo}) copied to a new draft with Id {newId}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Journal copied to a new editable draft.",
                Data = newId
            };
        }
    }
}
