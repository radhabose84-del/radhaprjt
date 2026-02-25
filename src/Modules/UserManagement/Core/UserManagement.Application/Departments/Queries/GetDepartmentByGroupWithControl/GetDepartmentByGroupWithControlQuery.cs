using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl
{
    public class GetDepartmentByGroupWithControlQuery : IRequest<ApiResponseDTO<List<DepartmentWithControlByGroupDto>>>
    {
        public string? DepartmentGroupName { get; set; }
    }
}