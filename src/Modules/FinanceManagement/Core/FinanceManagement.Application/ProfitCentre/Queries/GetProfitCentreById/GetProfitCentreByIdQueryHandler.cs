using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.ProfitCentre.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Queries.GetProfitCentreById
{
    public class GetProfitCentreByIdQueryHandler : IRequestHandler<GetProfitCentreByIdQuery, ProfitCentreDto?>
    {
        private readonly IProfitCentreQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProfitCentreByIdQueryHandler(IProfitCentreQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProfitCentreDto?> Handle(GetProfitCentreByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<ProfitCentreDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProfitCentreByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"ProfitCentre details {dto.Id} was fetched.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
