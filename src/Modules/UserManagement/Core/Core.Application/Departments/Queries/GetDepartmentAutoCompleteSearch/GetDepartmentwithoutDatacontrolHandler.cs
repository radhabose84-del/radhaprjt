using AutoMapper;
using Core.Application.Common.Interfaces;
using MediatR;
using Core.Application.Common.Interfaces.IDepartment;
using Core.Application.Departments.Queries.GetDepartments;
using Core.Domain.Events;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;

namespace Core.Application.Departments.Queries.GetDepartmentAutoCompleteSearch
{

    public class GetDepartmentwithoutDatacontrolHandler : IRequestHandler<GetDepartmentwithoutDataControl, ApiResponseDTO<List<DepartmentAutocompleteDto>>>
    {
        private readonly IDepartmentQueryRepository _departmentRepository;
        private readonly IMapper _mapper;

          private readonly IMediator _mediator; 

          private readonly ILogger<GetDepartmentwithoutDatacontrolHandler> _logger;
          private readonly IIPAddressService _ipAddressService;
        public GetDepartmentwithoutDatacontrolHandler(IDepartmentQueryRepository divisionRepository,IMapper mapper, IMediator mediator, ILogger<GetDepartmentwithoutDatacontrolHandler> logger,IIPAddressService ipAddressService)
        {
             _mapper =mapper;

            _departmentRepository = divisionRepository;    
            _mediator = mediator;        
            _logger = logger;
            _ipAddressService = ipAddressService;
        }


        public async Task<ApiResponseDTO<List<DepartmentAutocompleteDto>>> Handle(GetDepartmentwithoutDataControl request, CancellationToken cancellationToken)
        { 

                    var Adminresult = await _departmentRepository.GetDepartment_SuperAdmin(request.SearchPattern);
                    var AdmindeptDto = _mapper.Map<List<DepartmentAutocompleteDto>>(Adminresult);

                    return new ApiResponseDTO<List<DepartmentAutocompleteDto>>
                   {
                       IsSuccess = true,
                       Message = "Success",
                       Data = AdmindeptDto
                   }; 
                

            _logger.LogInformation($"Handling GetDepartmentAutoCompleteSearchQuery with search pattern: {request.SearchPattern}" );
              
             // Fetch departments matching the search pattern
                var result = await _departmentRepository.GetAllDepartmentAutoCompleteSearchAsync(request.SearchPattern);
                
                if (result is null || !result.Any())
                    {
                    _logger.LogWarning("No department records found in the database. Total count: {Count}", result?.Count ?? 0);

                        return new ApiResponseDTO<List<DepartmentAutocompleteDto>> { IsSuccess = false, Message = "No Record Found" };
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

                return new ApiResponseDTO<List<DepartmentAutocompleteDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = deptDto
                };
               

        }
    }
         
}

