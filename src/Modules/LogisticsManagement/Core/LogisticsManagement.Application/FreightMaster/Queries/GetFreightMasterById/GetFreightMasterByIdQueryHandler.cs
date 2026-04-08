using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterById
{
    public class GetFreightMasterByIdQueryHandler : IRequestHandler<GetFreightMasterByIdQuery, FreightMasterDto?>
    {
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFreightMasterByIdQueryHandler(IFreightMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<FreightMasterDto?> Handle(GetFreightMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<FreightMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetFreightMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"FreightMaster details {dto.Id} was fetched.",
                module: "FreightMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
