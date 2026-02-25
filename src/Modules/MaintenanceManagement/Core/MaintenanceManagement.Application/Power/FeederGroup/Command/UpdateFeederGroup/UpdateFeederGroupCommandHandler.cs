using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup
{
    public class UpdateFeederGroupCommandHandler : IRequestHandler<UpdateFeederGroupCommand, bool>
    {

        private readonly IFeederGroupCommandRepository _feederGroupCommandRepository;
        private readonly IFeederGroupQueryRepository _feederGroupQueryRepo;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        
        public UpdateFeederGroupCommandHandler( IFeederGroupCommandRepository feederGroupCommandRepository,  IFeederGroupQueryRepository feederGroupQueryRepo, IMediator imediator, IMapper imapper)
        {
            _feederGroupCommandRepository = feederGroupCommandRepository;
            _feederGroupQueryRepo = feederGroupQueryRepo;
             _imediator = imediator;
            _imapper = imapper;
        }

      public async Task<bool> Handle(UpdateFeederGroupCommand request, CancellationToken cancellationToken)
        {
            var feederGroup = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(request);

            if (request.IsActive == 0)
            {
                var linked = await _feederGroupQueryRepo.IsFeederGroupLinkedAsync(request.Id);
                if (linked)
                    throw new ValidationException("This master is linked with other records. You cannot inactivate this record.");
            }

            var result = await _feederGroupCommandRepository.UpdateAsync(request.Id, feederGroup);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: feederGroup.Id.ToString(),
                actionName: feederGroup.FeederGroupName ?? "NULL",
                details: "FeederGroup details were updated",
                module: "FeederGroup");

            await _imediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("FeederGroup Updation Failed.");
        }

    }
}