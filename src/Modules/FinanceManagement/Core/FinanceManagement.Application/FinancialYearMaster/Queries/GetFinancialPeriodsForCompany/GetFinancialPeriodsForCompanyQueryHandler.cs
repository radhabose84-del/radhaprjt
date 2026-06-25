using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialPeriodsForCompany
{
    public class GetFinancialPeriodsForCompanyQueryHandler : IRequestHandler<GetFinancialPeriodsForCompanyQuery, IReadOnlyList<FinancialPeriodMasterDto>>
    {
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFinancialPeriodsForCompanyQueryHandler(IFinancialYearMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<FinancialPeriodMasterDto>> Handle(GetFinancialPeriodsForCompanyQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPeriodsForCompanyAsync(request.CompanyId, cancellationToken);
            var dtos = _mapper.Map<List<FinancialPeriodMasterDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetFinancialPeriodsForCompanyQuery",
                actionName: dtos.Count.ToString(),
                details: $"FinancialPeriodMaster details fetched for Company {request.CompanyId}.",
                module: "FinancialPeriodMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
