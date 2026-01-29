using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption
{
    public class CreatePowerConsumptionCommandHandler : IRequestHandler<CreatePowerConsumptionCommand, int>
    {
        private readonly IPowerConsumptionCommandRepository _powerConsumptionCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreatePowerConsumptionCommandHandler(IPowerConsumptionCommandRepository powerConsumptionCommandRepository, IMediator imediator, IMapper imapper)
        {
            _powerConsumptionCommandRepository = powerConsumptionCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreatePowerConsumptionCommand request, CancellationToken cancellationToken)
        {
              // Calculate TotalUnits
            var totalUnits = (request.ClosingReading - request.OpeningReading) * 1000; //For Kw Conversion 
            var powerConsumption = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.PowerConsumption>(request);
            powerConsumption.TotalUnits = totalUnits;
            
            var result = await _powerConsumptionCommandRepository.CreateAsync(powerConsumption);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: powerConsumption.FeederTypeId.ToString(),
                actionName: powerConsumption.FeederId.ToString(),
                details: $"PowerConsumption details was created",
                module: "Power");
            await _imediator.Publish(domainEvent, cancellationToken);
          
                    
             return result > 0 ? result : throw new ExceptionRules("PowerConsumption Creation Failed.");
                 
        }
    }
}