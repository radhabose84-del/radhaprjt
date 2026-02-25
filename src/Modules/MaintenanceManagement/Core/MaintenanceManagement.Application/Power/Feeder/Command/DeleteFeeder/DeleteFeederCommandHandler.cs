using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Domain.Events;
using MediatR;
using FluentValidation;

namespace MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder
{
    public class DeleteFeederCommandHandler : IRequestHandler<DeleteFeederCommand, bool>
    {

        private readonly IFeederCommandRepository _feederCommandRepository;
        private readonly IFeederQueryRepository _feederQueryRepo;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;


        public DeleteFeederCommandHandler(IFeederCommandRepository feederCommandRepository, IFeederQueryRepository feederQueryRepo, IMapper imapper, IMediator imediator)
        {
            _feederCommandRepository = feederCommandRepository;
            _feederQueryRepo = feederQueryRepo;
            _imapper = imapper;
            _mediator = imediator;
        }
         public async Task<bool> Handle(DeleteFeederCommand request, CancellationToken cancellationToken)
        {
            var feeder = _imapper.Map<MaintenanceManagement.Domain.Entities.Power.Feeder>(request);

            
        var linked = await _feederQueryRepo.IsFeederLinkedAsync(request.Id);
        if (linked)
        {
         throw new ValidationException("This master is linked with other records. You cannot delete this record.");
        }     

            // Perform the delete operation
            var isDeleted = await _feederCommandRepository.DeleteAsync(request.Id,feeder);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: feeder.Id.ToString(),
                actionName: feeder.IsDeleted.ToString(),
                details: $"Feeder with ID {feeder.Id} was deleted.",
                module: "Feeder"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

         
            return isDeleted == true ? isDeleted : throw new ExceptionRules("Feeder deletion failed.");
           
        }
        
    }
}