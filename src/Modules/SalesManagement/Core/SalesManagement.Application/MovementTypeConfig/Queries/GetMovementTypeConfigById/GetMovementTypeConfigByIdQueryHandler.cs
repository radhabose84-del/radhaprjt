using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigById
{
    public class GetMovementTypeConfigByIdQueryHandler : IRequestHandler<GetMovementTypeConfigByIdQuery, MovementTypeConfigDto?>
    {
        private readonly IMovementTypeConfigQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMovementTypeConfigByIdQueryHandler(
            IMovementTypeConfigQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MovementTypeConfigDto?> Handle(GetMovementTypeConfigByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<MovementTypeConfigDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMovementTypeConfigByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"MovementTypeConfig details {dto.Id} was fetched.",
                module: "MovementTypeConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
