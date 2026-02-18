using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder
{
    public class UpdateFeederCommandHandler : IRequestHandler<UpdateFeederCommand, bool>
    {
        private readonly IFeederCommandRepository _feederCommandRepository;
        private readonly IFeederQueryRepository _feederQueryRepo;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public UpdateFeederCommandHandler(IFeederCommandRepository feederCommandRepository, IFeederQueryRepository feederQueryRepo, IMediator imediator, IMapper imapper)
        {
            _feederCommandRepository = feederCommandRepository;
            _feederQueryRepo = feederQueryRepo;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(UpdateFeederCommand request, CancellationToken cancellationToken)
        {
              var feeder = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(request);

            if (request.IsActive == 0)
            {
                var linked = await _feederQueryRepo.IsFeederLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }
            var result = await _feederCommandRepository.UpdateAsync(request.Id, feeder);


            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: feeder.Id.ToString(),
                actionName: "Update Feeder",
                details: "Feeder details were updated",
                module: "Feeder");

            await _imediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("Feeder Updation Failed.");
        }
    }
}