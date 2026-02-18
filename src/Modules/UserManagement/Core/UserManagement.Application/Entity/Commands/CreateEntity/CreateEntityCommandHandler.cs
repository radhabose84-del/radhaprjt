#nullable disable
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityLastCode;
using UserManagement.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common;
using UserManagement.Domain.Events;
using Contracts.Common;
using Serilog;
using FluentValidation;



namespace UserManagement.Application.Entity.Commands.CreateEntity
{
    public class CreateEntityCommandHandler :  IRequestHandler<CreateEntityCommand, int>
    {
        private readonly IEntityCommandRepository _IentityRepository;

 
        private readonly IMapper _Imapper;
        private readonly IMediator _Imediator;

         private readonly ILogger<CreateEntityCommandHandler> _logger;


         public CreateEntityCommandHandler(IEntityCommandRepository Ientityrepository, IMapper Imapper,IMediator Imediator,ILogger<CreateEntityCommandHandler> logger)
        {
            _IentityRepository = Ientityrepository;
            _Imapper = Imapper;
            _Imediator=Imediator;
             _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           
        }

  public async Task<int> Handle(CreateEntityCommand request, CancellationToken cancellationToken)
{
         // Check if Entity Name already exists
        var exists = await _IentityRepository.ExistsByCodeAsync(request.EntityName);
            if (exists)
            {
                 _logger.LogWarning($"Entity Name {request.EntityName} already exists.");
                 throw new ValidationException("Entity Name already exists.");
               
            }
        _logger.LogInformation($"Starting creation process for EntityCode: {request}");
        var entityCode = await _Imediator.Send(new GetEntityLastCodeQuery(), cancellationToken);
        _logger.LogInformation($"Completed creation process for EntityCode: {entityCode.Data}");

        if (entityCode.Data is null || string.IsNullOrEmpty(entityCode.Data))
        { 
            _logger.LogError($"Failed to create user for EntityCode: {entityCode.Data}");
            throw new Exception("Failed to generate entity code.");
            
        }
        // Map the request to the Core domain entity
        var entity = _Imapper.Map<UserManagement.Domain.Entities.Entity>(request);
        entity.EntityCode = entityCode.Data;

        // Log that the entity creation process is about to begin
        _logger.LogInformation($"Starting Entity creation process for EntityCode: {entity.EntityCode}");


            var result = await _IentityRepository.CreateAsync(entity);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.EntityCode,
                actionName: entity.EntityName,
                details: $"Entity '{entity.EntityName}' was created. EntityCode: {entity.Id}",
                module:"Entity"
            );
            await _Imediator.Publish(domainEvent, cancellationToken);
            _logger.LogInformation($"Entity creation process completed for EntityCode: {entity.EntityCode}");
            var entityDto = _Imapper.Map<GetEntityDTO>(entity);

             if (result > 0)
                  {
                     _logger.LogInformation("Entity {Entity} created successfully", result);
                        return result;
                 }
                 throw new Exception("Entity Creation Failed");
      
           

}

}
}