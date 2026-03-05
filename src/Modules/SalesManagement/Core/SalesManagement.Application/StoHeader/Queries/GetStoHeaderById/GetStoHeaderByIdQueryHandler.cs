using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Queries.GetStoHeaderById
{
    public class GetStoHeaderByIdQueryHandler : IRequestHandler<GetStoHeaderByIdQuery, StoHeaderDto?>
    {
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoHeaderByIdQueryHandler(IStoHeaderQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StoHeaderDto?> Handle(GetStoHeaderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<StoHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetStoHeaderByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"StoHeader details {dto.Id} was fetched.",
                module: "StoHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
