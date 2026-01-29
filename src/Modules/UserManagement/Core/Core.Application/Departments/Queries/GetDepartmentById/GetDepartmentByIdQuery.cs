
using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Departments.Queries.GetDepartments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Application.Departments.Queries.GetDepartmentById
{

    public class GetDepartmentByIdQuery :IRequest<GetDepartmentDto>
    {
        
        public int DepartmentId { get; set; }
        
    }
}