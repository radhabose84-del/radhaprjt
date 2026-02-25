using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentByDepartmentGroupId
{
    public class GetDepartmentsByDepartmentGroupIdQueryHandler : IRequestHandler<GetDepartmentsByDepartmentGroupIdQuery, ApiResponseDTO<List<DepartmentWithGroupDto>>>
    {
       private readonly IDepartmentQueryRepository _departmentRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;


        public GetDepartmentsByDepartmentGroupIdQueryHandler(IDepartmentQueryRepository departmentQueryRepository, IMapper mapper, IMediator mediator)
        {
            _departmentRepository = departmentQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

       public async Task<ApiResponseDTO<List<DepartmentWithGroupDto>>> Handle(GetDepartmentsByDepartmentGroupIdQuery request, CancellationToken cancellationToken)
        {
                  if (string.IsNullOrWhiteSpace(request.DepartmentGroupName))
                        {
                            return new ApiResponseDTO<List<DepartmentWithGroupDto>>
                            {
                                IsSuccess = false,
                                Message = "DepartmentGroupName is required.",
                                Data = new List<DepartmentWithGroupDto>()
                            };
                        }

                    var departments = await _departmentRepository.GetDepartmentsByDepartmentGroupIdAsync(request.DepartmentGroupName);

                if (departments == null)
                {
                    return new ApiResponseDTO<List<DepartmentWithGroupDto>>
                    {
                        IsSuccess = false,
                        Message = "Department not found.",
                       Data = new List<DepartmentWithGroupDto>()
                    };
                }              
               var dtoList = _mapper.Map<List<DepartmentWithGroupDto>>(departments);
                return new ApiResponseDTO<List<DepartmentWithGroupDto>>
                {
                     IsSuccess = true,
                    Message = "Departments retrieved successfully.",
                    Data = dtoList
                };
        }
    }
}