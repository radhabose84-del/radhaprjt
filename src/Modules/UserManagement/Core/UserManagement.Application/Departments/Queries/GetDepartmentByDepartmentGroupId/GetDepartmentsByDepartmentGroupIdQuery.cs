using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace UserManagement.Application.Departments.Queries.GetDepartmentByDepartmentGroupId
{
    public class GetDepartmentsByDepartmentGroupIdQuery : IRequest<ApiResponseDTO<List<DepartmentWithGroupDto>>>
    {
        public string? DepartmentGroupName { get; set; }
    }
}