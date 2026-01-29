using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Departments.Queries.GetDepartments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Departments.Commands.UpdateDepartment
{

    public class UpdateDepartmentCommand : IRequest<bool>
    {
        public int Id { get; set; }       
        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentGroupId { get; set; }
        public Status IsActive { get; set; }
             
    }
}