using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl
{
    public class GetDepartmentByGroupWithControlQuery : IRequest<ApiResponseDTO<List<DepartmentWithControlByGroupDto>>>
    {
        public string? DepartmentGroupName { get; set; }
    }
}