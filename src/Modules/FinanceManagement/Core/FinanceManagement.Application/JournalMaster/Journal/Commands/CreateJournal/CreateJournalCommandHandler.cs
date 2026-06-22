using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal
{
    public class CreateJournalCommandHandler : IRequestHandler<CreateJournalCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var period = await _queryRepository.GetOpenPeriodByDateAsync(companyId, request.VoucherDate)
                ?? throw new ExceptionRules("Voucher date is not within an open accounting period.");

            var statusId = await _queryRepository.GetStatusIdAsync("DRAFT");
            var sourceId = await _queryRepository.GetSourceIdAsync("MANUAL");

            var entity = _mapper.Map<FinanceManagement.Domain.Entities.JournalHeader>(request);
            entity.CompanyId = companyId;
            entity.UnitId = _ipAddressService.GetUnitId();
            entity.AccountingPeriodId = period.PeriodId;
            entity.FinancialYearId = period.FinancialYearId;
            entity.StatusId = statusId;
            entity.SourceId = sourceId;
            entity.IsReversal = false;
            entity.AutoApproved = false;

            entity.Details = BuildLines(request);
            entity.TotalDr = entity.Details.Sum(l => l.DrAmount);
            entity.TotalCr = entity.Details.Sum(l => l.CrAmount);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "JOURNAL_CREATE",
                actionName: newId.ToString(),
                details: $"Journal voucher draft created successfully with Id {newId}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Journal voucher saved as draft.",
                Data = newId
            };
        }

        private static List<FinanceManagement.Domain.Entities.JournalDetail> BuildLines(CreateJournalCommand request)
        {
            var lineNo = 0;
            return request.Lines.Select(l =>
            {
                var rate = l.ExchangeRate ?? 1m;
                return new FinanceManagement.Domain.Entities.JournalDetail
                {
                    LineNo = ++lineNo,
                    GlAccountId = l.GlAccountId,
                    DrAmount = l.DrAmount,
                    CrAmount = l.CrAmount,
                    CurrencyId = l.CurrencyId,
                    ExchangeRate = l.ExchangeRate,
                    BaseDrAmount = l.DrAmount * rate,
                    BaseCrAmount = l.CrAmount * rate,
                    CostCentreId = l.CostCentreId,
                    ProfitCentreId = l.ProfitCentreId,
                    LineNarration = l.LineNarration,
                    ReferenceDocNo = l.ReferenceDocNo
                };
            }).ToList();
        }
    }
}
