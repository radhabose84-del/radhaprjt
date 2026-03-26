using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Queries.GetYarnTypeById
{
    public class GetYarnTypeByIdQueryHandler : IRequestHandler<GetYarnTypeByIdQuery, YarnTypeDto>
    {
        private readonly IYarnTypeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnTypeByIdQueryHandler(
            IYarnTypeQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<YarnTypeDto> Handle(GetYarnTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<YarnTypeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetYarnTypeByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Yarn Type details {dto.Id} was fetched.",
                module: "YarnType"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
