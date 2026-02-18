using UserManagement.Application.Common;
using Contracts.Common;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Departments.Commands.CreateDepartment
{

    public class CreateDepartmentCommand : IRequest<DepartmentDto>
    {

        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }        
        public int DepartmentGroupId { get; set; }
         
    }
}