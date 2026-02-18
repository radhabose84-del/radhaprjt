#nullable disable
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using System.Data;
using UserManagement.Application.Common.Interfaces.IEntity;
using AutoMapper;
using UserManagement.Application.Common;
using UserManagement.Domain.Events;
using Contracts.Common;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.Entity.Queries.GetEntityById
{
    public class GetEntityByIdQueryHandler : IRequestHandler<GetEntityByIdQuery, GetEntityDTO>
    {
        private readonly IEntityQueryRepository _entityRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetEntityByIdQueryHandler> _logger;

    public GetEntityByIdQueryHandler(IEntityQueryRepository entityRepository,  IMapper mapper, IMediator mediator,ILogger<GetEntityByIdQueryHandler> logger)
    {
           _entityRepository = entityRepository;
           _mapper =mapper;
           _mediator = mediator;
           _logger = logger?? throw new ArgumentNullException(nameof(logger));
         
    }

    public async Task<GetEntityDTO>  Handle(GetEntityByIdQuery request, CancellationToken cancellationToken)
    {
                _logger.LogInformation($"Fetching Entity Request started: {request.EntityId}");
                 var entitylist = await _entityRepository.GetByIdAsync(request.EntityId);

                if (entitylist is null)
                {
                     _logger.LogWarning($"No Entity Record {request.EntityId} not found in DB.");
                     throw new ValidationException("Entity not found");
                    
                }
                var entityDto = _mapper.Map<GetEntityDTO>(entitylist);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetEntityByIdQuery",
                    actionCode: entityDto.EntityCode,      
                    actionName: entityDto.EntityName,              
                    details: $"Entity '{entityDto.EntityName}' was Fetched. EntityCode: {entityDto.EntityCode}",
                    module:"Entity"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogInformation($"Entity {entityDto.EntityName} Listed successfully.");
                return entityDto;
      
 
     }

    }
}