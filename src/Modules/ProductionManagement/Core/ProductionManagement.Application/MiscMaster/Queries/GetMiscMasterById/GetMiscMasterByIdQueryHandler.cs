using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQueryHandler : IRequestHandler<GetMiscMasterByIdQuery, MiscMasterDto?>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscMasterByIdQueryHandler(
            IMiscMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MiscMasterDto?> Handle(GetMiscMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<MiscMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMiscMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Misc Master details {dto.Id} was fetched.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
