using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Queries.GetCostCentreById
{
    public class GetCostCentreByIdQueryHandler : IRequestHandler<GetCostCentreByIdQuery, CostCentreDto?>
    {
        private readonly ICostCentreQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCostCentreByIdQueryHandler(ICostCentreQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CostCentreDto?> Handle(GetCostCentreByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<CostCentreDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCostCentreByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"CostCentre details {dto.Id} was fetched.",
                module: "CostCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
