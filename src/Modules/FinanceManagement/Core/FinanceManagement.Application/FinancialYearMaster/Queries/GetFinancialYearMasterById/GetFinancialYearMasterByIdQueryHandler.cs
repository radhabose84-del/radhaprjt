using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterById
{
    public class GetFinancialYearMasterByIdQueryHandler : IRequestHandler<GetFinancialYearMasterByIdQuery, FinancialYearMasterDto?>
    {
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFinancialYearMasterByIdQueryHandler(IFinancialYearMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<FinancialYearMasterDto?> Handle(GetFinancialYearMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<FinancialYearMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetFinancialYearMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"FinancialYearMaster details {dto.Id} was fetched.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
