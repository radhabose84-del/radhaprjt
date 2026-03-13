using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Queries.GetCountMasterById
{
    public class GetCountMasterByIdQueryHandler : IRequestHandler<GetCountMasterByIdQuery, CountMasterDto>
    {
        private readonly ICountMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCountMasterByIdQueryHandler(
            ICountMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CountMasterDto> Handle(GetCountMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var dto = _mapper.Map<CountMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCountMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Count Master details {dto.Id} was fetched.",
                module: "CountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
