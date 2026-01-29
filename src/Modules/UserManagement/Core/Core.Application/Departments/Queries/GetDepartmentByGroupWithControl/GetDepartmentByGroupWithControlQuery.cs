using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace Core.Application.Departments.Queries.GetDepartmentByGroupWithControl
{
    public class GetDepartmentByGroupWithControlQuery : IRequest<ApiResponseDTO<List<DepartmentWithControlByGroupDto>>>
    {
        public string? DepartmentGroupName { get; set; }
    }
}