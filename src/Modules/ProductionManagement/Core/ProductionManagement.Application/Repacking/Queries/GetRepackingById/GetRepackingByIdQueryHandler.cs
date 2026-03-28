using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Queries.GetRepackingById
{
    public class GetRepackingByIdQueryHandler : IRequestHandler<GetRepackingByIdQuery, RepackingHeaderDto>
    {
        private readonly IRepackingQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingByIdQueryHandler(
            IRepackingQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<RepackingHeaderDto> Handle(GetRepackingByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRepackingByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Repacking details {result.Id} was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
