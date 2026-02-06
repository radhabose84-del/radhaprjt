using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;


namespace UserManagement.Application.Departments.Commands.DeleteDepartment
{
 
    public class DeleteDepartmentCommand :IRequest<int>
    {
        public int Id { get; set; }
    }

   
}