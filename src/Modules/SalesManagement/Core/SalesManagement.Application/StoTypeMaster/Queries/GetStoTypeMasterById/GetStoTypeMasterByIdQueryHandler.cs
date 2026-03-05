using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoTypeMaster;
using SalesManagement.Application.StoTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoTypeMaster.Queries.GetStoTypeMasterById
{
    public class GetStoTypeMasterByIdQueryHandler : IRequestHandler<GetStoTypeMasterByIdQuery, StoTypeMasterDto?>
    {
        private readonly IStoTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoTypeMasterByIdQueryHandler(IStoTypeMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StoTypeMasterDto?> Handle(GetStoTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<StoTypeMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetStoTypeMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"StoTypeMaster details {dto.Id} was fetched.",
                module: "StoTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
