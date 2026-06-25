using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetPeriodForDate
{
    public class GetPeriodForDateQueryHandler : IRequestHandler<GetPeriodForDateQuery, FinancialPeriodMasterDto?>
    {
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPeriodForDateQueryHandler(
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<FinancialPeriodMasterDto?> Handle(GetPeriodForDateQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.GetPeriodForDateAsync(companyId, request.Date, cancellationToken);
            if (result == null) return null;

            var dto = _mapper.Map<FinancialPeriodMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetPeriodForDateQuery",
                actionName: request.Date.ToString("yyyy-MM-dd"),
                details: $"FinancialPeriodMaster resolved for date {request.Date:yyyy-MM-dd}.",
                module: "FinancialPeriodMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
