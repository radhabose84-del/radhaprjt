#nullable disable
using MediatR;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Common.Interfaces.IEntity;
using AutoMapper;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using FluentValidation;

namespace UserManagement.Application.Entity.Queries.GetEntityAutoComplete
{
    public class GetEntityAutocompleteQueryHandler : IRequestHandler<GetEntityAutocompleteQuery, List<EntityAutoCompleteDto>>
    {
        private readonly IEntityQueryRepository _entityRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 

        private readonly ILogger<GetEntityAutocompleteQueryHandler> _logger;
        private readonly IIPAddressService _ipAddressService;


    public GetEntityAutocompleteQueryHandler(IEntityQueryRepository entityRepository,  IMapper mapper,IMediator mediator,ILogger<GetEntityAutocompleteQueryHandler> logger,IIPAddressService ipAddressService)
    {
         _entityRepository = entityRepository;
         _mapper =mapper;
         _mediator = mediator;
         _logger = logger?? throw new ArgumentNullException(nameof(logger));
         _ipAddressService = ipAddressService;
    }

    public async Task<List<EntityAutoCompleteDto>> Handle(GetEntityAutocompleteQuery request, CancellationToken cancellationToken)
    {

            var groupcode = _ipAddressService.GetGroupCode();

            if(groupcode == "SUPER_ADMIN")
                {
                    var Adminresult = await _entityRepository.GetByEntityName_SuperAdmin(request.SearchPattern);
                    var Admindivision = _mapper.Map<List<EntityAutoCompleteDto>>(Adminresult);

                    return Admindivision; 
                }
                 _logger.LogInformation($"Search pattern started: {request.SearchPattern}");
                var entities = await _entityRepository.GetByEntityNameAsync(request.SearchPattern);

                if (entities is null || !entities.Any() || entities.Count == 0)
                {
                 _logger.LogWarning($"No Entity Record {request.SearchPattern} not found in DB.");
                 throw new ValidationException("No entity found");
                 
                }
                var entityDto = _mapper.Map<List<EntityAutoCompleteDto>>(entities);
                //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetEntityAutocompleteQuery",
                    actionCode:"Get Entity Autocomplete",        
                    actionName: request.SearchPattern,                
                    details: $"Entity '{request.SearchPattern}' was searched",
                    module:"Entity"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                 _logger.LogInformation($"Entity {entities.Count} Listed successfully.");
                return entityDto;
           
            }            

    }
    }
    
    
