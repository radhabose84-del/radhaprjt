using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumptionById
{
    public class GetPowerConsumptionByIdQueryHandler : IRequestHandler<GetPowerConsumptionByIdQuery, GetPowerConsumptionDto>
    {
        private readonly IPowerConsumptionQueryRepository _powerConsumptionQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPowerConsumptionByIdQueryHandler(IPowerConsumptionQueryRepository powerConsumptionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _powerConsumptionQueryRepository = powerConsumptionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GetPowerConsumptionDto> Handle(GetPowerConsumptionByIdQuery request, CancellationToken cancellationToken)
        {
              var result = await _powerConsumptionQueryRepository.GetPowerConsumptionById(request.Id);


            var feederGroupDto = _mapper.Map<GetPowerConsumptionDto>(result);

            // Domain Event: Audit Logging
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "POWERCONSUMPTION_VIEW",
                actionName: "View PowerConsumption",
                details: $"PowerConsumption details fetched for Id: {request.Id}",
                module: "Power"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return  feederGroupDto;
        }
    }
}