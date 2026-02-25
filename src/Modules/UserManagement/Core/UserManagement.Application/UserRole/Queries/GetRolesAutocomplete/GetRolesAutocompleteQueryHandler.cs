#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.UserRole.Queries.GetRolesAutocomplete
{
    public class GetRolesAutocompleteQueryHandler : IRequestHandler<GetRolesAutocompleteQuery, List<GetUserRoleAutocompleteDto>>
    {
        private readonly IUserRoleQueryRepository _userRoleRepository;
     private readonly IMapper _mapper;
       private readonly ILogger<GetRolesAutocompleteQueryHandler> _logger;
     private readonly IMediator _mediator;
     private readonly IIPAddressService _ipAddressService;

        public GetRolesAutocompleteQueryHandler(IUserRoleQueryRepository userRoleRepository, IMapper mapper, IMediator mediator,ILogger<GetRolesAutocompleteQueryHandler> logger,IIPAddressService ipAddressService)
        {
           _userRoleRepository = userRoleRepository;
            _mapper =mapper;

            _mediator=mediator;

            _logger = logger;
            _ipAddressService = ipAddressService;


        }

        public async Task<List<GetUserRoleAutocompleteDto>> Handle(GetRolesAutocompleteQuery request, CancellationToken cancellationToken)
        {
             var groupcode = _ipAddressService.GetGroupcode();

            if(groupcode == "SUPER_ADMIN" || groupcode == "ADMIN")
                {
                    var Adminresult = await _userRoleRepository.GetRoles_SuperAdmin(request.SearchTerm);
                    var AdminRoleDto = _mapper.Map<List<GetUserRoleAutocompleteDto>>(Adminresult);

                    return AdminRoleDto; 
                }

                  _logger.LogInformation($"Handling GetUserRoleAutoCompleteSearchQuery with search pattern: {request.SearchTerm}");

             // Fetch UserRole matching the search pattern
                var result = await _userRoleRepository.GetRolesAsync(request.SearchTerm);
                if (result is null || !result.Any())
                {
                    _logger.LogWarning($"No UserRole found for search pattern: {request.SearchTerm}" );
                    throw new ValidationException("No matching UserRole found");
                 
                }

                _logger.LogInformation($"UserRole found for search pattern: {request.SearchTerm}. Mapping results to DTO.");

                // Map the result to DTO
                var userRoleDto = _mapper.Map<List<GetUserRoleAutocompleteDto>>(result);

                // Publish domain event for audit logs
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode: "",
                    actionName: request.SearchTerm,
                    details: $"User Role '{request.SearchTerm}' was searched",
                    module: "User Role"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogInformation($"Domain event published for search pattern: {request.SearchTerm}");

                return userRoleDto;

               


    
        }
        
    }
}