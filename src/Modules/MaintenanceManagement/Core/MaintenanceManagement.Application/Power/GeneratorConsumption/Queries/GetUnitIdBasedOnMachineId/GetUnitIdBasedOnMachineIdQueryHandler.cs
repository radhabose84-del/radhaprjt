using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetUnitIdBasedOnMachineId
{
    public class GetUnitIdBasedOnMachineIdQueryHandler : IRequestHandler<GetUnitIdBasedOnMachineIdQuery, List<GetMachineIdBasedonUnitDto>>
    {
        private readonly IGeneratorConsumptionQueryRepository _generatorConsumptionQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUnitIdBasedOnMachineIdQueryHandler(IGeneratorConsumptionQueryRepository generatorConsumptionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _generatorConsumptionQueryRepository = generatorConsumptionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<GetMachineIdBasedonUnitDto>> Handle(GetUnitIdBasedOnMachineIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _generatorConsumptionQueryRepository.GetMachineIdBasedonUnit();
          
            var FeederSubFeeder = _mapper.Map<List<GetMachineIdBasedonUnitDto>>(result);
            //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetUnitIdBasedOnMachineIdQuery",
                    actionCode: "GetUnitIdBasedOnMachineIdQuery",        
                    actionName: "Machine Load", 
                    details: $"Machine details was fetched.",
                    module:"GeneratorConsumption"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  FeederSubFeeder;
        }
    }
}