
using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UserManagement.Application.Departments.Queries.GetDepartmentById
{

    public class GetDepartmentByIdQuery :IRequest<GetDepartmentDto>
    {
        
        public int DepartmentId { get; set; }
        
    }
}