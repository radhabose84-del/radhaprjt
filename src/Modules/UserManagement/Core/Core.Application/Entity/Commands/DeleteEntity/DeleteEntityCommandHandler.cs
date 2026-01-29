using AutoMapper;
using Core.Application.Common;

using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IEntity;
using Core.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Entity.Commands.DeleteEntity
{
    public class DeleteEntityCommandHandler  : IRequestHandler<DeleteEntityCommand,  int>
    {
        private readonly IEntityCommandRepository _ientityRepository;

        private readonly IEntityQueryRepository _IentityQueryRepository;
        private readonly IMapper _Imapper;
        private readonly ILogger<DeleteEntityCommandHandler> _logger;

        private readonly IMediator _mediator; 

        public DeleteEntityCommandHandler(IEntityCommandRepository Ientityrepository,IMapper Imapper,ILogger<DeleteEntityCommandHandler> logger,IMediator mediator,IEntityQueryRepository IentityQueryRepository)
        {
            _ientityRepository = Ientityrepository;
            _Imapper = Imapper;
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
            _IentityQueryRepository = IentityQueryRepository;
            
        }
        public async Task<int> Handle(DeleteEntityCommand request, CancellationToken cancellationToken)
        {       
       
       _logger.LogInformation($"Starting Entity Deletion process for EntityId: {request.EntityId}");
         // 🔹 First, check if the ID exists in the database
            var existingEntity = await _IentityQueryRepository.GetByIdAsync(request.EntityId);
            if (existingEntity is null )
            {
                _logger.LogWarning($"Entity ID {request.EntityId} not found.");
                throw new ValidationException("Entity Id not found / Entity is deleted .");
               
            }
        
        var entity = _Imapper.Map<Core.Domain.Entities.Entity>(request);

        var result = await _ientityRepository.DeleteEntityAsync(request.EntityId, entity);

        if (result == -1) 
        {
            _logger.LogInformation($"EntityId {request.EntityId} not found.");
            throw new ValidationException("Entity not found.");
            
        }
        //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Delete",
            actionCode: entity.Id.ToString(),
            actionName:"",
            details:$"EntityCode: {request.EntityId} was Changed to Status Inactive.",
            module:"Entity"
        );            
        await _mediator.Publish(domainEvent, cancellationToken);
        _logger.LogInformation($"Successfully completed Entity Deletion process for EntityId: {request.EntityId}");
         return  result;
   
}
         
   }
}