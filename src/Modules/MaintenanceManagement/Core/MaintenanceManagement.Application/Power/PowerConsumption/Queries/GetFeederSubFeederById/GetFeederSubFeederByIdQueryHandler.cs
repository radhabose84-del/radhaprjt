using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries
{
    public class GetFeederSubFeederByIdQueryHandler : IRequestHandler<GetFeederSubFeederByIdQuery, List<GetFeederSubFeederDto>>
    {
        private readonly IPowerConsumptionQueryRepository _powerConsumptionQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFeederSubFeederByIdQueryHandler(IPowerConsumptionQueryRepository powerConsumptionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _powerConsumptionQueryRepository = powerConsumptionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetFeederSubFeederDto>> Handle(GetFeederSubFeederByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _powerConsumptionQueryRepository.GetFeederSubFeedersById(request.FeederTypeId);
          
            var FeederSubFeeder = _mapper.Map<List<GetFeederSubFeederDto>>(result);
            //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetFeederSubFeederByIdQuery",
                    actionCode: "GetFeederSubFeederByIdQuery",        
                    actionName: "Feeders & SubFeeders Load", 
                    details: $"Feeder & SubFeeder details was fetched.",
                    module:"Feeder & SubFeeder"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  FeederSubFeeder;
        }
    }
}