using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Application.Departments.Queries.GetDepartments;
using Core.Application.Common;
using Core.Application.Common.HttpResponse;


namespace Core.Application.Departments.Commands.DeleteDepartment
{
 
    public class DeleteDepartmentCommand :IRequest<int>
    {
        public int Id { get; set; }
    }

   
}