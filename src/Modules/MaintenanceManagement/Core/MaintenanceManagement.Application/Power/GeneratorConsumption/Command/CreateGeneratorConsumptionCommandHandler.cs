using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Command
{
    public class CreateGeneratorConsumptionCommandHandler : IRequestHandler<CreateGeneratorConsumptionCommand, int>
    {
        private readonly IGeneratorConsumptionCommandRepository _generatorConsumptionCommandRepository; 
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateGeneratorConsumptionCommandHandler(IGeneratorConsumptionCommandRepository generatorConsumptionCommandRepository, IMediator imediator, IMapper imapper)
        {
            _generatorConsumptionCommandRepository = generatorConsumptionCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }


        public async Task<int> Handle(CreateGeneratorConsumptionCommand request, CancellationToken cancellationToken)
        {
            // Calculate TotalUnits
            var totalUnits = (request.ClosingEnergyReading - request.OpeningEnergyReading);  
            var generatorConsumption = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.GeneratorConsumption>(request);
            generatorConsumption.Energy = totalUnits;

            // Calculate running hours (as decimal with 3 precision)
            var duration = request.EndTime - request.StartTime;
            generatorConsumption.RunningHours = Math.Round((decimal)duration.TotalHours, 3); // Safe conversion

            
            var result = await _generatorConsumptionCommandRepository.CreateAsync(generatorConsumption);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: generatorConsumption.GeneratorId.ToString(),
                actionName: generatorConsumption.UnitId.ToString(),
                details: $"GeneratorConsumption details was created",
                module: "Generator");
            await _imediator.Publish(domainEvent, cancellationToken);
          
                    
             return result > 0 ? result : throw new ExceptionRules("GeneratorConsumption Creation Failed.");
        }
    }
}