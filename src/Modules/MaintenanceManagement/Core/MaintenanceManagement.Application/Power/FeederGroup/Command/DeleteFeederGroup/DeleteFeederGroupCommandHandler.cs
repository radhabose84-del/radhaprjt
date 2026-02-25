using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup
{
    public class DeleteFeederGroupCommandHandler : IRequestHandler<DeleteFeederGroupCommand, bool>
    {

        private readonly IFeederGroupCommandRepository _feederGroupCommandRepository;
        private readonly IFeederGroupQueryRepository _feederGroupQueryRepo;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        
        public DeleteFeederGroupCommandHandler( IFeederGroupCommandRepository feederGroupCommandRepository, IFeederGroupQueryRepository feederGroupQueryRepo, IMapper imapper, IMediator imediator)
        {
            _feederGroupCommandRepository = feederGroupCommandRepository;
            _feederGroupQueryRepo = feederGroupQueryRepo;
                _imapper = imapper;
                _mediator = imediator;
        }

        public async Task<bool> Handle(DeleteFeederGroupCommand request, CancellationToken cancellationToken)
        {
            var feederGroup = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.FeederGroup>(request);

            var linked = await _feederGroupQueryRepo.IsFeederGroupLinkedAsync(request.Id);
            if (linked)
            {
            throw new ValidationException("This master is linked with other records. You cannot delete this record.");
            }     

            // Perform the delete operation
            var isDeleted = await _feederGroupCommandRepository.DeleteAsync(request.Id,feederGroup);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: feederGroup.Id.ToString(),
                actionName: feederGroup.IsDeleted.ToString(),
                details: $"FeederGroup with ID {feederGroup.Id} was deleted.",
                module: "FeederGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

          
                return isDeleted == true ? isDeleted : throw new ExceptionRules("FeederGroup deletion failed.");
            
        }
    }
}