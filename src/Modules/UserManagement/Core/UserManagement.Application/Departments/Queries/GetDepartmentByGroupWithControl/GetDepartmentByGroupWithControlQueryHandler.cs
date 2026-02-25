#nullable disable
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IDepartment;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl
{
    public class GetDepartmentByGroupWithControlQueryHandler : IRequestHandler<GetDepartmentByGroupWithControlQuery, ApiResponseDTO<List<DepartmentWithControlByGroupDto>>>
    {
        private readonly IDepartmentQueryRepository _departmentRepository;
        private readonly IMapper _mapper;
         public GetDepartmentByGroupWithControlQueryHandler(IDepartmentQueryRepository departmentQueryRepository, IMapper mapper, IMediator mediator)
         {
             _departmentRepository = departmentQueryRepository;
            _mapper = mapper;
         }

        public async Task<ApiResponseDTO<List<DepartmentWithControlByGroupDto>>> Handle(GetDepartmentByGroupWithControlQuery request, CancellationToken cancellationToken)
        {
             var departments = await _departmentRepository.GetDepartmentsByDepartmentGroupWithControl(request.DepartmentGroupName);

                if (departments == null)
                {
                    return new ApiResponseDTO<List<DepartmentWithControlByGroupDto>>
                    {
                        IsSuccess = false,
                        Message = "Department not found.",
                       Data = new List<DepartmentWithControlByGroupDto>()
                    };
                }              
               var dtoList = _mapper.Map<List<DepartmentWithControlByGroupDto>>(departments);
                return new ApiResponseDTO<List<DepartmentWithControlByGroupDto>>
                {
                     IsSuccess = true,
                    Message = "Departments retrieved successfully.",
                    Data = dtoList
                };
        }
    }
}