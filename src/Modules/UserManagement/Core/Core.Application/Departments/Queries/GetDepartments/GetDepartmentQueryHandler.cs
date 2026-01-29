using AutoMapper;
using Core.Application.Common.Interfaces;
using MediatR;
using System.Data;
using Core.Domain.Events;
using Core.Application.Common.Interfaces.IDepartment;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Microsoft.Extensions.Logging;


namespace Core.Application.Departments.Queries.GetDepartments
{

    public class GetDepartmentQueryHandler :IRequestHandler<GetDepartmentQuery,ApiResponseDTO<List<GetDepartmentDto>>>
    {
        private readonly IDepartmentQueryRepository _departmentRepository;
        private readonly IMapper _mapper; 
         private readonly IMediator _mediator; 

         private readonly ILogger<GetDepartmentQueryHandler> _logger;


     public GetDepartmentQueryHandler(IDepartmentQueryRepository divisionRepository,IMapper mapper , IMediator mediator, ILogger<GetDepartmentQueryHandler> logger)
        {
            _mapper =mapper;
            _departmentRepository = divisionRepository; 
            _mediator = mediator;     
            _logger = logger;

        }
        // public async Task<ApiResponseDTO<List<GetDepartmentDto>>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
        //     {
        //         _logger.LogInformation("Fetching Department Request started. Page: {Page}, Size: {Size}, Search: {Search}", 
        //             request.PageNumber, request.PageSize, request.SearchTerm);

        //         var (departmentDynamicList, totalCount) = await _departmentRepository
        //             .GetAllDepartmentAsync(request.PageNumber, request.PageSize, request.SearchTerm);

        //         if (departmentDynamicList is null || !departmentDynamicList.Any())
        //         {
        //             _logger.LogWarning("No department records found in the database.");

        //             return new ApiResponseDTO<List<GetDepartmentDto>>
        //             {
        //                 IsSuccess = false,
        //                 Message = "No Record Found"
        //             };
        //         }

        //         // ✅ Map from dynamic to DTO
        //         var departmentList = _mapper.Map<List<GetDepartmentDto>>(departmentDynamicList);

        //      //   var mapped = _mapper.Map<List<GetDepartmentDto>>(departmentDtos);

        //         var domainEvent = new AuditLogsDomainEvent(
        //             actionDetail: "GetAll",
        //             actionCode: "",
        //             actionName: "",
        //             details: $"Fetched {departmentList.Count} department records.",
        //             module: "Department"
        //         );

        //         await _mediator.Publish(domainEvent, cancellationToken);

        //         _logger.LogInformation("Fetched {Count} departments successfully.", departmentList.Count);

        //         return new ApiResponseDTO<List<GetDepartmentDto>>
        //         {
        //             IsSuccess = true,
        //             Message = "Success",
        //             Data = departmentList,
        //             TotalCount = totalCount,
        //             PageNumber = request.PageNumber,
        //             PageSize = request.PageSize
        //         };
        //     }


        public async Task<ApiResponseDTO<List<GetDepartmentDto>>> Handle(GetDepartmentQuery request ,CancellationToken cancellationToken )
        {
             _logger.LogInformation("Fetching Department Request started: {request}", request);
           
            var (department,totalCount) = await _departmentRepository.GetAllDepartmentAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            
             if (department is null || !department.Any())
            {
             

                  return new ApiResponseDTO<List<GetDepartmentDto>> { IsSuccess = false, Message = "No Record Found" };
            }

             var departmentList = _mapper.Map<List<GetDepartmentDto>>(department);
             var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",        
                    actionName: "",
                    details: $"Department details was fetched.",
                    module:"Department"
                );

                  await _mediator.Publish(domainEvent, cancellationToken);
              
            _logger.LogInformation("Department {department} Listed successfully.", departmentList.Count);
            return new ApiResponseDTO<List<GetDepartmentDto>> { IsSuccess = true,
            Message = "Success",
            Data = departmentList ,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize};  
           
        }

      
    }
}