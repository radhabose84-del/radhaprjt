#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Entity.Commands.UpdateEntity
{
    public class UpdateEntityCommandHandler : IRequestHandler<UpdateEntityCommand, int>
    {
       private readonly IEntityCommandRepository _Ientityrepository;

       private readonly IEntityQueryRepository _IentityQueryRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<UpdateEntityCommandHandler> _logger;
        private readonly IMediator _mediator; 
       public UpdateEntityCommandHandler(IEntityCommandRepository Ientityrepository,IMapper Imapper, ILogger<UpdateEntityCommandHandler> logger,IMediator mediator,IEntityQueryRepository IentityQueryRepository)
        {
            _Ientityrepository = Ientityrepository;
            _Imapper = Imapper;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
            _IentityQueryRepository = IentityQueryRepository;
             
        }

       public async Task<int> Handle(UpdateEntityCommand request, CancellationToken cancellationToken)
        { 
            _logger.LogInformation($"Starting Entity Update process for EntityId: {request.Id}");

            // 🔹 First, check if the ID exists in the database
            var existingEntity = await _IentityQueryRepository.GetByIdAsync(request.Id);
            if (existingEntity is null)
            {
                _logger.LogWarning($"Entity ID {request.Id} not found.");
                throw new ValidationException("Entity Id not found / Entity is deleted .");
             
            }

            // 🔹 Check if entity name already exists for another ID
            var exists = await _Ientityrepository.ExistsByNameupdateAsync(request.EntityName, request.Id);
            if (exists)
            {
                _logger.LogWarning($"Entity Name {request.EntityName} already exists.");
                throw new ValidationException("Entity Name already exists.");
               
            }

            var entity = _Imapper.Map<UserManagement.Domain.Entities.Entity>(request);
            var result = await _Ientityrepository.UpdateAsync (request.Id, entity);

            if (result == -1) // Entity not found
            {
                _logger.LogInformation($"EntityId {request.Id} not found.");
                throw new ValidationException("Entity not found.");
                
            }

              //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: entity.Id.ToString(),
            actionName: entity.EntityName,                            
            details:$"Entity '{entity.EntityName}' was Updated. EntityCode: {request.Id}",
            module:"Entity"
            );            
            await _mediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Successfully completed Entity Update process for EntityId: {request.Id}");
            return  result;

        
        
        }
     }
}