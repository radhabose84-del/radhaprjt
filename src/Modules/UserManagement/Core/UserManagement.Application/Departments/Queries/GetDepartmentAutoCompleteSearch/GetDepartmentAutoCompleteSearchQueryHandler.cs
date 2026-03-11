#nullable disable
using AutoMapper;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Events;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch
{

    public class GetDepartmentAutoCompleteSearchQueryHandler : IRequestHandler<GetDepartmentAutoCompleteSearchQuery, List<DepartmentAutocompleteDto>>
    {
        private readonly IDepartmentQueryRepository _departmentRepository;
        private readonly IMapper _mapper;

          private readonly IMediator _mediator; 

          private readonly ILogger<GetDepartmentAutoCompleteSearchQueryHandler> _logger;
          private readonly IIPAddressService _ipAddressService;
        public GetDepartmentAutoCompleteSearchQueryHandler(IDepartmentQueryRepository divisionRepository,IMapper mapper, IMediator mediator, ILogger<GetDepartmentAutoCompleteSearchQueryHandler> logger,IIPAddressService ipAddressService)
        {
             _mapper =mapper;

            _departmentRepository = divisionRepository;    
            _mediator = mediator;        
            _logger = logger;
            _ipAddressService = ipAddressService;
        }


        public async Task<List<DepartmentAutocompleteDto>> Handle(GetDepartmentAutoCompleteSearchQuery request, CancellationToken cancellationToken)
        { 

            var groupcode = _ipAddressService.GetGroupCode();

            if(groupcode == "SUPER_ADMIN" || groupcode == "ADMIN")
                {
                    var Adminresult = await _departmentRepository.GetDepartment_SuperAdmin(request.SearchPattern);
                    var AdmindeptDto = _mapper.Map<List<DepartmentAutocompleteDto>>(Adminresult);

                    return  AdmindeptDto; 
                }

            _logger.LogInformation($"Handling GetDepartmentAutoCompleteSearchQuery with search pattern: {request.SearchPattern}" );
              
             // Fetch departments matching the search pattern
                var result = await _departmentRepository.GetAllDepartmentAutoCompleteSearchAsync(request.SearchPattern);
                
                if (result is null || !result.Any())
                    {
                    _logger.LogWarning("No department records found in the database. Total count: {Count}", result?.Count ?? 0);
                    throw new ValidationException("No Record Found");
                        
                    }
                _logger.LogInformation($"Departments found for search pattern: {request.SearchPattern}. Mapping results to DTO.");

                // Map the result to DTO
                var deptDto = _mapper.Map<List<DepartmentAutocompleteDto>>(result);

                // Publish domain event for audit logs
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAutoComplete",
                    actionCode: "",
                    actionName: request.SearchPattern,
                    details: $"Department '{request.SearchPattern}' was searched",
                    module: "Department"
                );
                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogInformation($"Domain event published for search pattern: {request.SearchPattern}");

                return deptDto;
               

        }
    }
         
}

