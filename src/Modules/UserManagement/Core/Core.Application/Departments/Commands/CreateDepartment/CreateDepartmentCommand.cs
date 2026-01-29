using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Departments.Queries.GetDepartments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Departments.Commands.CreateDepartment
{

    public class CreateDepartmentCommand : IRequest<DepartmentDto>
    {

        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }        
        public int DepartmentGroupId { get; set; }
         
    }
}