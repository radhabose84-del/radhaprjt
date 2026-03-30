using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountGroup.Queries.GetCountGroupById
{
    public class GetCountGroupByIdQueryHandler : IRequestHandler<GetCountGroupByIdQuery, CountGroupDto>
    {
        private readonly ICountGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCountGroupByIdQueryHandler(
            ICountGroupQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CountGroupDto> Handle(GetCountGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<CountGroupDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCountGroupByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Count Group details {dto.Id} was fetched.",
                module: "CountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
