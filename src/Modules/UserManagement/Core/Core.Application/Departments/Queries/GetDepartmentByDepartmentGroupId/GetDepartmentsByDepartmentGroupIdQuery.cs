using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace Core.Application.Departments.Queries.GetDepartmentByDepartmentGroupId
{
    public class GetDepartmentsByDepartmentGroupIdQuery : IRequest<ApiResponseDTO<List<DepartmentWithGroupDto>>>
    {
        public string? DepartmentGroupName { get; set; }
    }
}